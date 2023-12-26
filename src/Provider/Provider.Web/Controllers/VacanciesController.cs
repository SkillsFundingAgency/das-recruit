using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    public class VacanciesController : Controller
    {
        private readonly VacanciesOrchestrator _orchestrator;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ServiceParameters _serviceParameters;
        private readonly IConfiguration _configuration;

        public VacanciesController(VacanciesOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment, ServiceParameters serviceParameters, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
            _serviceParameters = serviceParameters;
            _configuration = configuration;
        }

        [HttpGet("", Name = RouteNames.Vacancies_Get)]
        public async Task<IActionResult> Vacancies([FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
        {
            if (_serviceParameters.VacancyType == VacancyType.Traineeship 
                && DateTime.TryParse(_configuration["TraineeshipCutOffDate"], out var traineeshipCutOffDate))
            {
                if (traineeshipCutOffDate != DateTime.MinValue && traineeshipCutOffDate < DateTime.UtcNow)
                {
                    return RedirectPermanent(_configuration["ProviderSharedUIConfiguration:DashboardUrl"]);
                }
            }
            
            if (string.IsNullOrWhiteSpace(filter) && string.IsNullOrWhiteSpace(searchTerm))
                TryGetFiltersFromCookie(out filter, out searchTerm);
            
            if(string.IsNullOrWhiteSpace(filter) == false || string.IsNullOrWhiteSpace(searchTerm) == false)
                SaveFiltersInCookie(filter, searchTerm);

            var vm = await _orchestrator.GetVacanciesViewModelAsync(User.ToVacancyUser(), filter, page, searchTerm);
            if (TempData.ContainsKey(TempDataKeys.VacanciesErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.VacanciesErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.VacanciesInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.VacanciesInfoMessage].ToString();

            return View(vm);
        }

        private void SaveFiltersInCookie(string filter, string search)
        {
            var value = JsonConvert.SerializeObject(new FilterCookie(filter, search));
            Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.VacanciesFilter, value);
        }

        private void TryGetFiltersFromCookie(out string filter, out string search)
        {
            filter = string.Empty;
            search = string.Empty;
            var cookieValue = Request.Cookies.GetCookie(CookieNames.VacanciesFilter);
            if (string.IsNullOrWhiteSpace(cookieValue)) return;
            var values = JsonConvert.DeserializeObject<FilterCookie>(cookieValue);
            filter = values.Filter;
            search = values.SearchTerm;
        }

        private class FilterCookie
        {
            public string Filter { get; set; }
            public string SearchTerm { get; set; }
            public FilterCookie() { }
            public FilterCookie(string filter, string searchTerm)
            {
                Filter = filter;
                SearchTerm = searchTerm;
            }
        }
    }
}