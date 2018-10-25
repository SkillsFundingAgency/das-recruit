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

            SetSectionStates(viewModel);

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
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m);

            SetSectionStates(viewModel);

            return View(ViewNames.VacancyPreview, viewModel);
        }

        private void SetSectionStates(VacancyPreviewViewModel viewModel)
        {
            viewModel.TitleSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Title }, true, vm => vm.Title);
            viewModel.ShortDescriptionSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ShortDescription }, true, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ClosingDate }, true, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.WorkingWeek }, true, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Wage }, true, vm => vm.HasWage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ExpectedDuration }, true, vm => vm.HasWage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.PossibleStartDate }, true, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.TrainingLevel }, true, vm => vm.HasProgramme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.NumberOfPositions }, true, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.VacancyDescription, VacancyReview.FieldIdentifiers.TrainingDescription, VacancyReview.FieldIdentifiers.OutcomeDescription }, true, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Skills }, true, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Qualifications }, true, vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ThingsToConsider }, true, vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetSectionState(viewModel, null, true, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerDescription }, true, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerWebsiteUrl }, true, vm => vm.EmployerWebsiteUrl);
            viewModel.ContactSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerContact }, false, vm => vm.ContactName, vm => vm.ContactEmail, vm => vm.ContactTelephone);
            viewModel.EmployerAddressSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.EmployerAddress }, true, vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ApplicationInstructions }, true, vm => vm.ApplicationInstructions);
            viewModel.ApplicationMethodSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ApplicationMethod }, true, vm => vm.ApplicationMethod);
            viewModel.ApplicationUrlSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.ApplicationUrl }, true, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Provider }, true, vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetSectionState(viewModel, new[] { VacancyReview.FieldIdentifiers.Training }, true, vm => vm.TrainingType, vm => vm.TrainingTitle);
            viewModel.DisabilityConfidentSectionState = GetSectionState(viewModel, new[]{ VacancyReview.FieldIdentifiers.DisabilityConfident}, true, vm => vm.IsDisabilityConfident);
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
    }
}