using System;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    public class TraineeshipHomeController: Controller
    {
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public TraineeshipHomeController(ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }
        [HttpGet("traineeship-help", Name = RouteNames.TraineeshipHelp)]
        public IActionResult Help()
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            
            return View();
        }
        
        [HttpGet("traineeship-privacy", Name = RouteNames.TraineeshipPrivacy)]
        public IActionResult Privacy()
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            return View();
        }
        
        [HttpGet("traineeship-terms-and-conditions", Name = RouteNames.TraineeshipTermsAndConditions)]
        public IActionResult TermsAndConditions()
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            
            return View();
        }

        
        
        [HttpGet("traineeship-cookies", Name = RouteNames.TraineeshipCookies)]
        public IActionResult Cookies()
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            return View();
        }
        
        [HttpGet("traineeship-cookies-details", Name = RouteNames.TraineeshipCookiesDetails)]
        public IActionResult CookiesDetails()
        {
            if (IsTraineeshipsDisabled())
            {
                return RedirectPermanent($"{_configuration["ProviderSharedUIConfiguration:DashboardUrl"]}account");
            }
            return View();
        }
        
        private bool IsTraineeshipsDisabled()
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return true;
                }
            }
            return false;
        }
    }
}