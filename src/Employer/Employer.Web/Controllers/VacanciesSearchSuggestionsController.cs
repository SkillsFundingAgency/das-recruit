using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class VacanciesSearchSuggestionsController : Controller
    {
        private readonly VacanciesSearchSuggestionsOrchestrator _orchestrator;
        public VacanciesSearchSuggestionsController(VacanciesSearchSuggestionsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("search-suggestions", Name = RouteNames.VacanciesSearchSuggestions_Get)]
        public async Task<JsonResult> GetSearchSuggestions([FromQuery]string term, VacancyRouteModel model)
        {    
            var data = await _orchestrator.GetSearchSuggestionsAsync(term, model.EmployerAccountId);
            return Json(data);
        }
    }
}