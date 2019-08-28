using System;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class DashboardController : Controller
    {
        private readonly DashboardOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public DashboardController(DashboardOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("", Name = RouteNames.Dashboard_Index_Get)]
        public async Task<IActionResult> Dashboard([FromRoute] string employerAccountId, [FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(filter) && string.IsNullOrWhiteSpace(searchTerm))
                TryGetFiltersFromCookie(out filter, out searchTerm);

            if (string.IsNullOrWhiteSpace(filter) == false || string.IsNullOrWhiteSpace(searchTerm) == false)
                SaveFiltersInCookie(filter, searchTerm);

            var vm = await _orchestrator.GetDashboardViewModelAsync(employerAccountId, filter, page, User.ToVacancyUser(), searchTerm);

            if (TempData.ContainsKey(TempDataKeys.DashboardErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.DashboardErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.DashboardInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.DashboardInfoMessage].ToString();

            return View(vm);
        }

        private void SaveFiltersInCookie(string filter, string search)
        {
            var value = JsonConvert.SerializeObject(new FilterCookie(filter, search));
            Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.DashboardFilter, value);
        }

        private void TryGetFiltersFromCookie(out string filter, out string search)
        {
            filter = string.Empty;
            search = string.Empty;
            var cookieValue = Request.Cookies.GetCookie(CookieNames.DashboardFilter);
            if (string.IsNullOrWhiteSpace(cookieValue)) return;
            try
            {
                var values = JsonConvert.DeserializeObject<FilterCookie>(cookieValue);
                filter = values.Filter;
                search = values.SearchTerm;
            }
            catch (JsonException)
            {
                //As the cookie value was initially set as string, we need to handle the deserialization in a try/catch block.
            }
        }

        private class FilterCookie
        {
            public string Filter { get; }
            public string SearchTerm { get; }
            public FilterCookie(string filter, string searchTerm)
            {
                Filter = filter;
                SearchTerm = searchTerm;
            }
        }

        [HttpPost("dismiss-alert", Name = RouteNames.Dashboard_DismissAlert_Post)]
        public async Task<IActionResult> DismissAlert([FromRoute] string employerAccountId, AlertDismissalEditModel model)
        {
            if (Enum.TryParse<AlertType>(model.AlertType, out var alertTypeEnum))
            {
                await _orchestrator.DismissAlert(User.ToVacancyUser(), alertTypeEnum);
            }

            return RedirectToRoute(RouteNames.Dashboard_Index_Get);
        }
    }
}