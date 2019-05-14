using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class VacanciesSearchSuggestionsController : Controller
    {
        private readonly VacanciesSearchSuggestionsOrchestrator _orchestrator;
        public VacanciesSearchSuggestionsController(VacanciesSearchSuggestionsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("search-helper", Name = RouteNames.SearchHelper_Get)]
        public async Task<JsonResult> GetAutoCompleteList([FromQuery]string term, VacancyRouteModel model)
        {    
            var data = await _orchestrator.GetAutoCompleteListAsync(term, model.Ukprn);
            return Json(data);
        }
    }
}