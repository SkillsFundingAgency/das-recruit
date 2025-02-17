using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Middleware;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerOrTransactorAccount))]
    public class VacancyPreviewController : Controller
    {
        private readonly VacancyPreviewOrchestrator _orchestrator;

        public VacancyPreviewController(VacancyPreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("advert-preview", Name = RouteNames.VacancyAdvertPreview)]
        public async Task<IActionResult> AdvertPreview(VacancyRouteModel vrm, bool? submitToEfsa = null)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vrm);
            AddSoftValidationErrorsToModelState(viewModel);
            viewModel.SetSectionStates(viewModel, ModelState);

            viewModel.CanHideValidationSummary = true;
            viewModel.SubmitToEsfa = submitToEfsa;

            if (TempData.ContainsKey(TempDataKeys.VacancyClonedInfoMessage))
                viewModel.VacancyClonedInfoMessage = TempData[TempDataKeys.VacancyClonedInfoMessage].ToString();

            return View(viewModel);
        }
        
        [HttpGet("preview", Name = RouteNames.Vacancy_Preview_Get)]
        public async Task<IActionResult> VacancyPreview(VacancyRouteModel vrm, bool? submitToEfsa = null)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vrm);
            AddSoftValidationErrorsToModelState(viewModel);
            viewModel.SetSectionStates(viewModel, ModelState);

            viewModel.CanHideValidationSummary = true;
            viewModel.SubmitToEsfa = submitToEfsa;

            if (TempData.ContainsKey(TempDataKeys.VacancyClonedInfoMessage))
                viewModel.VacancyClonedInfoMessage = TempData[TempDataKeys.VacancyClonedInfoMessage].ToString();

            return View(viewModel);
        }

        [HttpPost("review", Name = RouteNames.Preview_Review_Post)]
        public async Task<IActionResult> Review(SubmitReviewModel m)
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
                    : RouteNames.RejectJobAdvert_Get,
                    new {m.VacancyId, m.EmployerAccountId});
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);
            viewModel.SoftValidationErrors = null;
            viewModel.SubmitToEsfa = m.SubmitToEsfa;
            viewModel.RejectedReason = m.RejectedReason;
            viewModel.SetSectionStates(viewModel, ModelState);

            return View(ViewNames.VacancyPreview, viewModel);
        }

        [HttpPost("preview", Name = RouteNames.Preview_Submit_Post)]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var response = await _orchestrator.SubmitVacancyAsync(m, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (ModelState.IsValid)
            {
                if (response.Data.IsSubmitted)
                    return RedirectToRoute(RouteNames.Submitted_Index_Get, new {m.VacancyId, m.EmployerAccountId});

                if (response.Data.HasLegalEntityAgreement == false)
                    return RedirectToRoute(RouteNames.LegalEntityAgreement_HardStop_Get, new {m.VacancyId, m.EmployerAccountId});

                throw new Exception("Unknown submit state");
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);
            viewModel.SoftValidationErrors = null;
            viewModel.SetSectionStates(viewModel, ModelState);

            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {m.VacancyId, m.EmployerAccountId});
        }

        [HttpGet("approve-advert", Name = RouteNames.ApproveJobAdvert_Get)]
        public IActionResult ApproveJobAdvert(VacancyRouteModel vm)
        {              
            return View(new ApproveJobAdvertViewModel());
        }

        [HttpPost("approve-advert", Name = RouteNames.ApproveJobAdvert_Post)]
        public async Task<IActionResult> ApproveJobAdvert(ApproveJobAdvertViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("ApproveJobAdvert");
            }

            if (vm.ApproveJobAdvert.GetValueOrDefault())
            {
                var response = await _orchestrator.ApproveJobAdvertAsync(vm, User.ToVacancyUser());
                if (!response.Success)
                {
                    response.AddErrorsToModelState(ModelState);
                }

                if (ModelState.IsValid)
                {
                    if (response.Data.IsSubmitted)
                        return RedirectToRoute(RouteNames.JobAdvertConfirmation_Get, new {vm.VacancyId, vm.EmployerAccountId});

                    if (response.Data.HasLegalEntityAgreement == false)
                        return RedirectToRoute(RouteNames.LegalEntityAgreement_HardStop_Get, new {vm.VacancyId, vm.EmployerAccountId});

                    throw new Exception("Unknown submit state");
                }
            }
            else
            {
                return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vm.VacancyId, vm.EmployerAccountId});
            }
            
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vm.VacancyId, vm.EmployerAccountId});
        }

        [HttpGet("reject-advert", Name = RouteNames.RejectJobAdvert_Get)]
        public async Task<IActionResult> RejectJobAdvert(VacancyRouteModel vm)
        {
            var viewModel = await _orchestrator.GetVacancyRejectJobAdvertAsync(vm);          

            return View(viewModel);
        }


        [HttpPost("reject-advert", Name = RouteNames.RejectJobAdvert_Post)]
        public async Task<IActionResult> RejectJobAdvert(RejectJobAdvertViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await _orchestrator.GetVacancyRejectJobAdvertAsync(vm);
                return View("RejectJobAdvert", viewModel);
            }

            if (vm.RejectJobAdvert.GetValueOrDefault())
            {                
                var response =  await _orchestrator.RejectJobAdvertAsync(vm, User.ToVacancyUser());
                if (!response.Success)
                {
                    response.AddErrorsToModelState(ModelState);
                }

                if (response.Data.IsRejected)
                {
                    return RedirectToRoute(RouteNames.JobAdvertConfirmation_Get, new {vm.VacancyId, vm.EmployerAccountId});
                }
            }
            
            return RedirectToRoute(RouteNames.EmployerCheckYourAnswersGet, new {vm.VacancyId, vm.EmployerAccountId});
            }

        [HttpGet("confirmation-advert", Name = RouteNames.JobAdvertConfirmation_Get)]
        public async Task<IActionResult> ConfirmationJobAdvert(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyConfirmationJobAdvertAsync(vrm);

            return View("ConfirmationJobAdvert", viewModel);
        }

        
        private void AddSoftValidationErrorsToModelState(VacancyPreviewViewModel viewModel)
        {
            if (!viewModel.SoftValidationErrors.HasErrors)
                return;

            foreach (var error in viewModel.SoftValidationErrors.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}