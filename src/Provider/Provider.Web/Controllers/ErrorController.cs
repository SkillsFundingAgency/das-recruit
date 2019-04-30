using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Error;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly ExternalLinksConfiguration _externalLinks;

        public ErrorController(ILogger<ErrorController> logger, IOptions<ExternalLinksConfiguration> externalLinks)
        {
            _logger = logger;
            _externalLinks = externalLinks.Value;
        }

        [Route("error/{id?}")]
        public IActionResult Error(int id)
        {
            ViewBag.IsErrorPage = true; // Used by layout to show/hide elements.

            switch (id)
            {
                case 403:
                    return AccessDenied();
                case 404:
                    return PageNotFound();
                default:
                    break;
            }

            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route(RoutePaths.ExceptionHandlingPath)]
        public IActionResult ErrorHandler()
        {
            if (HttpContext.Items.TryGetValue(ContextItemKeys.ProviderIdentifier, out var ukprn))
            {
                if (!RouteData.Values.ContainsKey(ContextItemKeys.ProviderIdentifier))
                    RouteData.Values.Add(ContextItemKeys.ProviderIdentifier, ukprn);
            }

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                string routeWhereExceptionOccurred = exceptionFeature.Path;
                var exception = exceptionFeature.Error;

                if (exception is AggregateException aggregateException)
                {
                    var flattenedExceptions = aggregateException.Flatten();
                    _logger.LogError(flattenedExceptions, "Aggregate exception on path: {route}", routeWhereExceptionOccurred);

                    exception = flattenedExceptions.InnerExceptions.FirstOrDefault();
                }

                if (exception is InvalidStateException)
                {
                    _logger.LogError(exception, "Exception on path: {route}", routeWhereExceptionOccurred);
                    AddDashboardMessage(exception.Message);
                    return RedirectToRoute(RouteNames.Vacancies_Get, new { Ukprn = ukprn });
                }

                if (exception is InvalidRouteForVacancyException invalidRouteException)
                {
                    return RedirectToRoute(invalidRouteException.RouteNameToRedirectTo, invalidRouteException.RouteValues);
                }

                if (exception is VacancyNotFoundException)
                {
                    return PageNotFound();
                }

                if (exception is AuthorisationException)
                {   
                    return AccessDenied();
                }

                if (exception is ReportNotFoundException)
                {
                    return PageNotFound();
                }

                if (exception is BlockedProviderException)
                {
                    return RedirectToRoute(RouteNames.BlockedProvider_Get, new { Ukprn = ukprn });
                }

                if (exception is MissingPermissionsException mpEx)
                {
                    _logger.LogWarning(mpEx.Message);
                    return MissingPermissions(long.Parse((string)ukprn));
                }

                _logger.LogError(exception, "Unhandled exception on path: {route}", routeWhereExceptionOccurred);
            }

            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View(ViewNames.ErrorView, new ErrorViewModel { StatusCode = (int)HttpStatusCode.InternalServerError, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private IActionResult MissingPermissions(long ukprn)
        {
            var vm = new MissingPermissionsViewModel
            {
                RouteValues = new VacancyRouteModel { Ukprn = ukprn },
                CtaRoute = RouteNames.Vacancies_Get
            };

            return View(ViewNames.MissingPermissions, vm);
        }

        private IActionResult AccessDenied()
        {
            var serviceClaim = User.FindFirst(ProviderRecruitClaims.IdamsUserServiceTypeClaimTypeIdentifier);
            
            if (serviceClaim == null || serviceClaim.Value != ProviderRecruitClaims.ServiceClaimValue)
            {
                _logger.LogInformation("User does not have service claim");
                return Redirect(_externalLinks.ProviderApprenticeshipSiteUrl);
            }

            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(ViewNames.AccessDenied);
        }

        private IActionResult PageNotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View(ViewNames.PageNotFound);
        }       

        [HttpGet("error/provider/{ukprn}", Name = RouteNames.BlockedProvider_Get)]
        public IActionResult BlockedProvider(string ukprn)
        {
            _logger.LogInformation($"Handling redirection of blocked provider: {ukprn}.");
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(ViewNames.ProviderBlocked);
        }

        private void AddDashboardMessage(string message)
        {
            if(TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
                _logger.LogError($"Dashboard message already set in {nameof(ErrorController)}. Existing message:{TempData[TempDataKeys.DashboardErrorMessage]}. New message:{message}");

            TempData[TempDataKeys.DashboardErrorMessage] = message;
        }
    }
}