using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Esfa.Recruit.Employer.Web.ViewModels.Error;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Diagnostics;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Models;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Exceptions;
using System.Net;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [AllowAnonymous]    
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly DashboardOrchestrator _orchestrator;

        public ErrorController(ILogger<ErrorController> logger, DashboardOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;
        }

        [Route("Error/{id?}")]
        public async Task<IActionResult> Error(int id)
        {
            return View(new ErrorViewModel { StatusCode = id, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/handle")]
        public async Task<IActionResult> ErrorHandler()
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
                    var vm = await _orchestrator.GetDashboardViewModelAsync(employerDetail);
                    ModelState.AddModelError(string.Empty, exception.Message);

                    return View(ViewNames.DashboardView, vm);
                }
            }

            return View(ViewNames.ErrorView, new ErrorViewModel { StatusCode = (int)HttpStatusCode.InternalServerError, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
