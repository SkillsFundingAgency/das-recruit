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
        public async Task<IActionResult> Vacancies([FromQuery] string filter, [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(filter))
                filter = Request.Cookies.GetCookie(CookieNames.VacanciesFilter);

            if (string.IsNullOrWhiteSpace(filter) == false)
                Response.Cookies.SetSessionCookie(_hostingEnvironment, CookieNames.VacanciesFilter, filter);

            var vm = await _orchestrator.GetVacanciesViewModelAsync(User.GetUkprn(), filter, page);
            if (TempData.ContainsKey(TempDataKeys.VacanciesErrorMessage))
                vm.WarningMessage = TempData[TempDataKeys.VacanciesErrorMessage].ToString();

            if (TempData.ContainsKey(TempDataKeys.VacanciesInfoMessage))
                vm.InfoMessage = TempData[TempDataKeys.VacanciesInfoMessage].ToString();

            return View(vm);
        }
    }
}