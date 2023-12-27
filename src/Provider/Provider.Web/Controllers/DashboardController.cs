using System;
using System.Globalization;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public DashboardController(DashboardOrchestrator orchestrator, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Get)]
        public async Task<IActionResult> Dashboard()
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
                }
            }
            
            var vm = await _orchestrator.GetDashboardViewModelAsync(User.ToVacancyUser());
            return View(vm.HasEmployerReviewPermission ? ViewNames.DashboardWithReview : ViewNames.DashboardNoReview, vm);
        }
    }
}