using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.Controllers
{
    [Route(RoutePaths.AccountRoutePath)]
    public class SearchHelperController : Controller
    {
        private readonly SearchHelperOrchestrator _orchestrator;
        public SearchHelperController(SearchHelperOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("search-helper")]
        public async Task<JsonResult> GetAutoCompleteList([FromQuery]string term, VacancyRouteModel model)
        {    
            var data = await _orchestrator.GetAutoCompleteListAsync(term, model.Ukprn);
            return Json(data);
        }
    }
}