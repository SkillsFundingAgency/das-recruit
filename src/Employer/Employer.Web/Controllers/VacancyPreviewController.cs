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
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.Configuration;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePrefixPaths.AccountVacancyRoutePath)]
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

            SetViewSectionStates(viewModel);

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
                return RedirectToRoute(RouteNames.Submitted_Index_Get);
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);

            SetSubmitSectionStates(viewModel);

            return View(ViewNames.VacancyPreview, viewModel);
        }

        private void SetViewSectionStates(VacancyPreviewViewModel viewModel)
        {
            viewModel.TitleSectionState = GetViewSectionState(viewModel, new [] {VacancyReview.FieldIdentifiers.Title}, true, vm => vm.Title);
            viewModel.ShortDescriptionSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ShortDescription }, true, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ClosingDate }, true, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.WorkingWeek }, true, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Wage }, true, vm => vm.HasWage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ExpectedDuration }, true, vm => vm.HasWage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.PossibleStartDate }, true, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.TrainingLevel }, true, vm => vm.HasProgramme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.NumberOfPositions }, true, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetViewSectionState(viewModel, new []{ VacancyReview.FieldIdentifiers.VacancyDescription, VacancyReview.FieldIdentifiers.TrainingDescription, VacancyReview.FieldIdentifiers.OutcomeDescription }, true, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Skills }, true, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Qualifications }, true, vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ThingsToConsider }, true, vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetViewSectionState(viewModel, null, true, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerDescription }, true, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerWebsiteUrl }, true, vm => vm.EmployerWebsiteUrl);
            viewModel.ContactSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerContact }, false, vm => vm.ContactName, vm => vm.ContactEmail, vm => vm.ContactTelephone);
            viewModel.EmployerAddressSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerAddress }, true, vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ApplicationInstructions }, true, vm => vm.ApplicationInstructions);
            viewModel.ApplicationMethodSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ApplicationMethod }, true, vm => vm.ApplicationMethod);
            viewModel.ApplicationUrlSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ApplicationUrl }, true, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Provider }, true, vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetViewSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Training}, true, vm => vm.TrainingType, vm => vm.TrainingTitle);
            viewModel.DisabilityConfidentSectionState = GetViewSectionState(viewModel, VacancyReview.FieldIdentifiers.DisabilityConfident);
        }

        private VacancyPreviewSectionState GetViewSectionState(VacancyPreviewViewModel vm, string reviewFieldIndicator)
        {
            if (vm.ReviewFieldIndicators.Select(r => r.ReviewFieldIdentifier).Contains(reviewFieldIndicator))
                return VacancyPreviewSectionState.Review;

            return VacancyPreviewSectionState.Valid;
        }

        /// <summary>
        /// Returns the state of the section for viewing. Incomplete, Valid or Review
        /// </summary>
        /// <param name="vm">The View Model to inspect</param>
        /// <param name="reviewFieldIndicators">If any of these indicators exist in vm.ReviewFieldIndicators then immediately returns 'Review'</param>
        /// <param name="requiresAll">If 'true' requires all of the requiredProperties to be incomplete to return 'Incomplete'. If false will return 'Incomplete' if any requiredProperties are incomplete</param>
        /// <param name="requiredProperties">The list of properties to inspect to determine if the state is 'Valid' or 'Incomplete'</param>
        /// <returns>Incomplete, Valid or Review</returns>
        private VacancyPreviewSectionState GetViewSectionState(VacancyPreviewViewModel vm, IEnumerable<string> reviewFieldIndicators, bool requiresAll, params Func<VacancyPreviewViewModel, object>[] requiredProperties)
        {
            if (reviewFieldIndicators != null && reviewFieldIndicators.Any(reviewFieldIndicator => 
                vm.ReviewFieldIndicators.Select(r => r.ReviewFieldIdentifier)
                    .Contains(reviewFieldIndicator)))
            {
                return VacancyPreviewSectionState.Review;
            }
            
            foreach (var requiredPropertyFunc in requiredProperties)
            {
                VacancyPreviewSectionState result = VacancyPreviewSectionState.Valid;
                var propertyValue = requiredPropertyFunc(vm);

                switch (propertyValue)
                {
                    case null:
                        result = VacancyPreviewSectionState.Incomplete;
                        break;
                    case string stringProperty:
                        if (string.IsNullOrWhiteSpace(stringProperty))
                        {
                            result = VacancyPreviewSectionState.Incomplete;
                        }
                        break;
                    case IEnumerable listProperty:
                        if (listProperty.Cast<object>().Any() == false)
                        {
                            result = VacancyPreviewSectionState.Incomplete;
                        }
                        break;
                }

                if (requiresAll && result == VacancyPreviewSectionState.Incomplete)
                {
                    return result;
                }

                if (!requiresAll && result == VacancyPreviewSectionState.Valid)
                {
                    return result;
                }
            }

            return requiresAll ? VacancyPreviewSectionState.Valid : VacancyPreviewSectionState.Incomplete;
        }

        private void SetSubmitSectionStates(VacancyPreviewViewModel viewModel)
        {
            viewModel.ShortDescriptionSectionState = GetSubmitSectionState(vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetSubmitSectionState(vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetSubmitSectionState(vm => vm.HasWage, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetSubmitSectionState(vm => vm.HasWage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetSubmitSectionState(vm => vm.HasWage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetSubmitSectionState(vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetSubmitSectionState(vm => vm.HasProgramme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetSubmitSectionState(vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetSubmitSectionState(vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetSubmitSectionState(vm => vm.Skills);
            viewModel.QualificationsSectionState = GetSubmitSectionState(vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = GetSubmitSectionState(vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetSubmitSectionState(vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetSubmitSectionState(vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetSubmitSectionState(vm => vm.EmployerWebsiteUrl);
            viewModel.ContactSectionState = GetSubmitSectionState(vm => vm.ContactName, vm => vm.ContactEmail, vm => vm.ContactTelephone);
            viewModel.EmployerAddressSectionState = GetSubmitSectionState(vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = GetSubmitSectionState(vm => vm.ApplicationInstructions);
            viewModel.ApplicationMethodSectionState = GetSubmitSectionState(vm => vm.ApplicationMethod);
            viewModel.ApplicationUrlSectionState = GetSubmitSectionState(vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetSubmitSectionState(vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetSubmitSectionState(vm => vm.HasProgramme, vm => vm.TrainingType, vm => vm.TrainingTitle);
        }

        private VacancyPreviewSectionState GetSubmitSectionState(params Expression<Func<VacancyPreviewViewModel, object>>[] validProperties)
        {
            foreach (var property in validProperties)
            {
                var propertyName = property.GetPropertyName();
                if (ModelState.Keys.Any(k => k == propertyName && ModelState[k].Errors.Any()))
                {
                    return VacancyPreviewSectionState.Invalid;
                }
            }
            
            return VacancyPreviewSectionState.Valid;
        }
    }
}