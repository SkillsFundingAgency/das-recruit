using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
    public class VacanciesController : Controller
    {
        private readonly VacanciesOrchestrator _orchestrator;
        private readonly IHostingEnvironment _hostingEnvironment;

        public VacanciesController(VacanciesOrchestrator orchestrator, IHostingEnvironment hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("", Name = RouteNames.Vacancies_Get)]
        public async Task<IActionResult> Vacancies(
            [FromQuery] string filter, [FromQuery] int page = 1, [FromQuery] string searchTerm = "")
        {
            if (string.IsNullOrWhiteSpace(filter) && string.IsNullOrWhiteSpace(searchTerm))
                TryGetFiltersFromCookie(out filter, out searchTerm);
            
            if(string.IsNullOrWhiteSpace(filter) == false || string.IsNullOrWhiteSpace(searchTerm) == false)
                SaveFiltersInCookie(filter, searchTerm);

            var vm = await _orchestrator.GetVacanciesViewModelAsync(User.GetUkprn(), filter, page, searchTerm);
            if (TempData.ContainsKey(TempDataKeys.VacanciesErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.VacanciesErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.VacanciesInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.VacanciesInfoMessage].ToString();

            return View(vm);
        }

        private void SaveFiltersInCookie(string filter, string search)
        {
            var value = $"filter:{filter}||search:{search}";
            Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.VacanciesFilter, value);
        }

        private void TryGetFiltersFromCookie(out string filter, out string search)
        {
            filter = string.Empty;
            search = string.Empty;
            var cookieValue = Request.Cookies.GetCookie(CookieNames.VacanciesFilter);
            if(string.IsNullOrWhiteSpace(cookieValue)) return;
            var values = cookieValue.Split("||");
            if (values.Length != 2) return;
            filter = values[0].Replace("filter:", "");
            search = values[1].Replace("search:", "");
        }
    }
}