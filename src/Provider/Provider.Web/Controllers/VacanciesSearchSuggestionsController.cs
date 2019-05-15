using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.VacanciesRoutePath)]
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
            var data = await _orchestrator.GetSearchSuggestionsAsync(term, model.Ukprn);
            return Json(data);
        }
    }
}