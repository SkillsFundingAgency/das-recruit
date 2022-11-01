using System.Diagnostics;
using System.Net;
using Esfa.Recruit.Qa.Web.Configuration;
using Esfa.Recruit.Qa.Web.Configuration.Routing;
using Esfa.Recruit.Qa.Web.Exceptions;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("error/{id?}")]
        public IActionResult Error(int id)
        {
            switch (id)
            {
                case 403:
                    return AccessDenied();
                case 404:
                    return PageNotFound();
            }

            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route(RoutePaths.ExceptionHandlingPath)]
        public IActionResult ErrorHandler()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exception = exceptionFeature.Error;

                switch (exception)
                {
                    case NotFoundException _:
                        _logger.LogError(exception, "Exception on path: {route}", exceptionFeature.Path);
                        Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return View(ViewNames.ErrorView, GetViewModel(HttpStatusCode.NotFound));

                    case ReportNotFoundException _:
                    case VacancyNotFoundException _:
                        return PageNotFound();

                    case UnassignedVacancyReviewException _:
                        TempData.Add(TempDataKeys.DashboardMessage, exception.Message);
                        return RedirectToRoute(RouteNames.Dashboard_Index_Get);

                    default:
                        break;
                }

                _logger.LogError(exception, "Unhandled exception on path: {route}", exceptionFeature.Path);
            }

            return View(ViewNames.ErrorView, GetViewModel(HttpStatusCode.InternalServerError));
        }

        private ErrorViewModel GetViewModel(HttpStatusCode statusCode)
        {
            return new ErrorViewModel { StatusCode = (int)statusCode, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        }

        private IActionResult AccessDenied()
        {
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return View(ViewNames.AccessDenied);
        }

        private IActionResult PageNotFound()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View(ViewNames.PageNotFound);
        }
    }
}