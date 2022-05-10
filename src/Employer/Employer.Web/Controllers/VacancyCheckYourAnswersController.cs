using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyCheckYourAnswersController : Controller
    {
        private readonly VacancyTaskListOrchestrator _orchestrator;

        public VacancyCheckYourAnswersController (VacancyTaskListOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }
        
        [HttpGet("check-answers", Name = RouteNames.EmployerCheckYourAnswersGet)]
        
        public async Task<IActionResult> CheckYourAnswers(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyTaskListModel(vrm); 
            
            viewModel.SetSectionStates(viewModel, ModelState);
            
            return View(viewModel);
        }
        
        
        [HttpPost("check-answers-review", Name = RouteNames.EmployerCheckYourAnswersPost)]
        public async Task<IActionResult> CheckYourAnswers(SubmitReviewModel m)
        {
            if (ModelState.IsValid)
            {
                if(m.SubmitToEsfa.Value)
                {
                    await _orchestrator.ClearRejectedVacancyReason(m, User.ToVacancyUser());
                }
                else
                {
                    await _orchestrator.UpdateRejectedVacancyReason(m, User.ToVacancyUser());
                }

                return RedirectToRoute(m.SubmitToEsfa.GetValueOrDefault()
                    ? RouteNames.ApproveJobAdvert_Get
                    : RouteNames.RejectJobAdvert_Get);
            }

            var viewModel = await _orchestrator.GetVacancyTaskListModel(new VacancyRouteModel
            {
                VacancyId = m.VacancyId,
                EmployerAccountId = m.EmployerAccountId
            });
            viewModel.SoftValidationErrors = null;
            viewModel.SubmitToEsfa = m.SubmitToEsfa;
            viewModel.RejectedReason = m.RejectedReason;
            viewModel.SetSectionStates(viewModel, ModelState);
            
            return View(viewModel);
        }
    }
}