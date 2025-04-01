using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Esfa.Recruit.Shared.Web;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerOrTransactorAccount))]
    public class VacancyCheckYourAnswersController(VacancyTaskListOrchestrator orchestrator) : Controller
    {
        private static readonly Dictionary<string, Tuple<string, string>> ValidationMappings = new()
        {
            { "EmployerLocations", Tuple.Create<string, string>("EmployerAddress", null) },
            { VacancyValidationErrorCodes.AddressCountryNotInEngland, Tuple.Create("EmployerAddress", "Location must be in England. Your apprenticeship must be in England to advertise it on this service") },
            { $"Multiple-{VacancyValidationErrorCodes.AddressCountryNotInEngland}", Tuple.Create("EmployerAddress", "All locations must be in England. Your apprenticeship must be in England to advertise it on this service") },
        };

        [HttpGet("check-answers", Name = RouteNames.EmployerCheckYourAnswersGet)]
        public async Task<IActionResult> CheckYourAnswers(VacancyRouteModel vrm)
        {
            var viewModel = await orchestrator.GetVacancyTaskListModel(vrm); 
            viewModel.CanHideValidationSummary = true;
            viewModel.SetSectionStates(viewModel, ModelState);
            
            if (TempData.ContainsKey(TempDataKeys.VacancyClonedInfoMessage))
                viewModel.VacancyClonedInfoMessage = TempData[TempDataKeys.VacancyClonedInfoMessage].ToString();
            
            return View(viewModel);
        }
        
        [HttpPost("check-answers-review", Name = RouteNames.EmployerCheckYourAnswersPost)]
        public async Task<IActionResult> CheckYourAnswers(SubmitReviewModel m)
        {
            if (ModelState.IsValid)
            {
                if(m.SubmitToEsfa.Value)
                {
                    await orchestrator.ClearRejectedVacancyReason(m, User.ToVacancyUser());
                }
                else
                {
                    await orchestrator.UpdateRejectedVacancyReason(m, User.ToVacancyUser());
                }

                return RedirectToRoute(m.SubmitToEsfa.GetValueOrDefault()
                    ? RouteNames.ApproveJobAdvert_Get
                    : RouteNames.RejectJobAdvert_Get, 
                    new {m.VacancyId, m.EmployerAccountId});
            }

            var viewModel = await orchestrator.GetVacancyTaskListModel(new VacancyRouteModel
            {
                VacancyId = m.VacancyId,
                EmployerAccountId = m.EmployerAccountId
            });
            viewModel.SoftValidationErrors = null;
            viewModel.SubmitToEsfa = m.SubmitToEsfa;
            viewModel.RejectedReason = m.RejectedReason;
            viewModel.SetSectionStates(viewModel, ModelState);
            viewModel.ValidationErrors = new ValidationSummaryViewModel
                {ModelState = ModelState, OrderedFieldNames = viewModel.OrderedFieldNames};
            return View(viewModel);
        }
        
        [HttpPost("check-answers", Name = RouteNames.EmployerCheckYourAnswersSubmitPost)]
        public async Task<IActionResult> CheckYourAnswers(SubmitEditModel m)
        {
            var response = await orchestrator.SubmitVacancyAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                ModelState.AddValidationErrorsWithMappings(response.Errors, ValidationMappings);
            }

            if (ModelState.IsValid)
            {
                if (response.Data.IsSubmitted)
                    return RedirectToRoute(RouteNames.Submitted_Index_Get, new {m.VacancyId, m.EmployerAccountId});

                if (response.Data.HasLegalEntityAgreement == false)
                    return RedirectToRoute(RouteNames.LegalEntityAgreement_HardStop_Get, new {m.VacancyId, m.EmployerAccountId});

                throw new Exception("Unknown submit state");
            }

            var viewModel = await orchestrator.GetVacancyTaskListModel(m);
            viewModel.SoftValidationErrors = null;
            viewModel.SetSectionStates(viewModel, ModelState);
            viewModel.ValidationErrors = new ValidationSummaryViewModel
                {ModelState = ModelState, OrderedFieldNames = viewModel.OrderedFieldNames};
            return View(viewModel);
        }
    }
}