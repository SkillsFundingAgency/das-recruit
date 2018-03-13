using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Esfa.Recruit.Employer.Web.ViewModels.Error;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Models;
using System.Net;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.Configuration.Routing;

namespace Esfa.Recruit.Employer.Web.Controllers
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
            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/handle")]
        public IActionResult ErrorHandler()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var employerDetail = (EmployerIdentifier)HttpContext.Items[ContextItemKeys.EmployerIdentifier];
                string routeWhereExceptionOccurred = exceptionFeature.Path;
                var exception = exceptionFeature.Error;

                if (exception is ConcurrencyException)
                {
                    _logger.LogError(exception, $"Exception on path: {routeWhereExceptionOccurred}");                    
                    TempData.Add(TempDataKeys.DashboardErrorMessage, exception.Message);
                    return RedirectToRoute(RouteNames.Dashboard_Index_Get, new { EmployerAccountId = employerDetail.AccountId });
                }
            }

            return View(ViewNames.ErrorView, new ErrorViewModel { StatusCode = (int)HttpStatusCode.InternalServerError, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
