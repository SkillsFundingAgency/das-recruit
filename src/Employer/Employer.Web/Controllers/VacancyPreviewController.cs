using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class VacancyPreviewController : Controller
    {
        private readonly VacancyPreviewOrchestrator _orchestrator;

        public VacancyPreviewController(VacancyPreviewOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("preview", Name = RouteNames.Vacancy_Preview_Get)]
        public async Task<IActionResult> VacancyPreview(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vrm);
            AddSoftValidationErrorsToModelState(viewModel);
            SetSectionStates(viewModel);

            viewModel.CanHideValidationSummary = true;

            if (TempData.ContainsKey(TempDataKeys.VacancyClonedInfoMessage))
                viewModel.VacancyClonedInfoMessage = TempData[TempDataKeys.VacancyClonedInfoMessage].ToString();

            return View(viewModel);
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
                    return RedirectToRoute(RouteNames.Submitted_Index_Get);

                if (response.Data.HasLegalEntityAgreement == false)
                    return RedirectToRoute(RouteNames.LegalEntityAgreement_HardStop_Get);

                throw new Exception("Unknown submit state");
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);
            viewModel.SoftValidationErrors = null;
            SetSectionStates(viewModel);

            return View(ViewNames.VacancyPreview, viewModel);
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

            if ((bool)vm.ApproveJobAdvert)
            {
                var response = await _orchestrator.ApproveJobAdvertAsync(vm, User.ToVacancyUser());
                if (!response.Success)
                {
                    response.AddErrorsToModelState(ModelState);
                }

                if (ModelState.IsValid)
                {
                    if (response.Data.IsSubmitted)
                        return RedirectToRoute(RouteNames.JobAdvertConfirmation_Get);

                    if (response.Data.HasLegalEntityAgreement == false)
                        return RedirectToRoute(RouteNames.LegalEntityAgreement_HardStop_Get);

                    throw new Exception("Unknown submit state");
                }
            }
            else
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new { VacancyId = vm.VacancyId });
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vm);
            viewModel.SoftValidationErrors = null;
            SetSectionStates(viewModel);

            return View(ViewNames.VacancyPreview, viewModel);
        }

        [HttpGet("reject-advert", Name = RouteNames.RejectJobAdvert_Get)]
        public IActionResult RejectJobAdvert(VacancyRouteModel vm)
        {           
            return View(new RejectJobAdvertViewModel());
        }


        [HttpPost("reject-advert", Name = RouteNames.RejectJobAdvert_Post)]
        public async Task<IActionResult> RejectJobAdvert(RejectJobAdvertViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("RejectJobAdvert");
            }

            if ((bool)vm.RejectJobAdvert)
            {
                //TODO : update the status of the Vacancy to Rejected                
            }
            else
            {
                return RedirectToRoute(RouteNames.Vacancy_Preview_Get, new { VacancyId = vm.VacancyId });
            }

            return RedirectToRoute(RouteNames.JobAdvertConfirmation_Get);
        }

        [HttpGet("confirmation-advert", Name = RouteNames.JobAdvertConfirmation_Get)]
        public async Task<IActionResult> ConfirmationJobAdvert(VacancyRouteModel vrm)
        {
            var viewModel = await _orchestrator.GetVacancyConfirmationJobAdvertAsync(vrm);

            return View("ConfirmationJobAdvert", viewModel);
        }

        private void SetSectionStates(VacancyPreviewViewModel viewModel)
        {
            viewModel.TitleSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Title }, true, vm => vm.Title);
            viewModel.ShortDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ShortDescription }, true, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ClosingDate }, true, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.WorkingWeek }, true, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Wage }, true, vm => vm.HasWage, vm => vm.WageText);
            viewModel.ExpectedDurationSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ExpectedDuration }, true, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.PossibleStartDate }, true, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.TrainingLevel }, true, vm => vm.HasProgramme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.NumberOfPositions }, true, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.VacancyDescription, FieldIdentifiers.TrainingDescription, FieldIdentifiers.OutcomeDescription }, true, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Skills }, true, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Qualifications }, true, vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ThingsToConsider }, true, vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerName }, true, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerDescription }, true, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerWebsiteUrl }, true, vm => vm.EmployerWebsiteUrl);
            viewModel.EmployerContactSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerContact }, false, vm => vm.EmployerContactName, vm => vm.EmployerContactEmail, vm => vm.EmployerContactTelephone);
            viewModel.EmployerAddressSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.EmployerAddress }, true, vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ApplicationInstructions }, true, vm => vm.ApplicationInstructions);
            viewModel.ApplicationMethodSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ApplicationMethod }, true, vm => vm.ApplicationMethod);
            viewModel.ApplicationUrlSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.ApplicationUrl }, true, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Provider }, true, vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetSectionState(viewModel, new[] { FieldIdentifiers.Training }, true, vm => vm.TrainingType, vm => vm.TrainingTitle);
            viewModel.DisabilityConfidentSectionState = GetSectionState(viewModel, new[]{ FieldIdentifiers.DisabilityConfident}, true, vm => vm.IsDisabilityConfident);
        }

        private VacancyPreviewSectionState GetSectionState(VacancyPreviewViewModel vm, IEnumerable<string> reviewFieldIndicators, bool requiresAll, params Expression<Func<VacancyPreviewViewModel, object>>[] sectionProperties)
        {
            if (IsSectionModelStateValid(sectionProperties) == false)
            {
                return IsSectionComplete(vm, requiresAll, sectionProperties) ? 
                    VacancyPreviewSectionState.Invalid : 
                    VacancyPreviewSectionState.InvalidIncomplete;
            }

            if (IsSectionForReview(vm, reviewFieldIndicators))
                return VacancyPreviewSectionState.Review;

            return IsSectionComplete(vm, requiresAll, sectionProperties) ?
                VacancyPreviewSectionState.Valid :
                VacancyPreviewSectionState.Incomplete;
        }

        private bool IsSectionModelStateValid(params Expression<Func<VacancyPreviewViewModel, object>>[] sectionProperties)
        {
            if (ModelState.IsValid)
                return true;

            foreach (var property in sectionProperties)
            {
                var propertyName = property.GetPropertyName();
                if (ModelState.Keys.Any(k => k == propertyName && ModelState[k].Errors.Any()))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSectionComplete(VacancyPreviewViewModel vm, bool requiresAll, params Expression<Func<VacancyPreviewViewModel, object>>[] sectionProperties)
        {
            foreach (var requiredPropertyExpression in sectionProperties)
            {
                var requiredPropertyFunc = requiredPropertyExpression.Compile();
                var propertyValue = requiredPropertyFunc(vm);

                var result = true;
                switch (propertyValue)
                {
                    case null:
                        result = false;
                        break;
                    case string stringProperty:
                        if (string.IsNullOrWhiteSpace(stringProperty))
                        {
                            result = false;
                        }
                        break;
                    case IEnumerable listProperty:
                        if (listProperty.Cast<object>().Any() == false)
                        {
                            result = false;
                        }
                        break;
                    case bool _:
                        //No way to tell if a bool has been 'completed' so just skip
                        break;
                    default:
                        //Skipping other types for now
                        break;
                }

                if (requiresAll && result == false)
                {
                    return false;
                }

                if (!requiresAll && result)
                {
                    return true;
                }
            }

            return requiresAll;
        }

        private bool IsSectionForReview(VacancyPreviewViewModel vm, IEnumerable<string> reviewFieldIndicators)
        {
            return reviewFieldIndicators != null && reviewFieldIndicators.Any(reviewFieldIndicator =>
                       vm.Review.FieldIndicators.Select(r => r.ReviewFieldIdentifier)
                           .Contains(reviewFieldIndicator));
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