using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Employer.Web.RouteModel;

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

            return View("VacancyPreview", viewModel);
        }

        private void SetViewSectionStates(VacancyPreviewViewModel viewModel)
        {
            viewModel.ShortDescriptionSectionState = GetViewSectionState(viewModel, true, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetViewSectionState(viewModel, true, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetViewSectionState(viewModel, true, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetViewSectionState(viewModel, true, vm => vm.Wage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetViewSectionState(viewModel, true, vm => vm.Wage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetViewSectionState(viewModel, true, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetViewSectionState(viewModel, true, vm => vm.Programme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetViewSectionState(viewModel, true, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetViewSectionState(viewModel, true, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetViewSectionState(viewModel, true, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetViewSectionState(viewModel, true, vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = GetViewSectionState(viewModel, true, vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetViewSectionState(viewModel, true, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetViewSectionState(viewModel, true, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetViewSectionState(viewModel, true, vm => vm.EmployerWebsiteUrl);
            viewModel.ContactSectionState = GetViewSectionState(viewModel, false, vm => vm.ContactName, vm => vm.ContactEmail, vm => vm.ContactTelephone);
            viewModel.EmployerAddressSectionState = GetViewSectionState(viewModel, true, vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = GetViewSectionState(viewModel, true, vm => vm.ApplicationInstructions);
            viewModel.ApplicationMethodSectionState = GetViewSectionState(viewModel, true, vm => vm.ApplicationMethod);
            viewModel.ApplicationUrlSectionState = GetViewSectionState(viewModel, true, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetViewSectionState(viewModel, true, vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetViewSectionState(viewModel, true, vm => vm.TrainingType, vm => vm.TrainingTitle);
        }
        
        private VacancyPreviewSectionState GetViewSectionState<T>(T vm, bool requiresAll, params Func<T, object>[] requiredProperties)
        {
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
            viewModel.WorkingWeekSectionState = GetSubmitSectionState(vm => vm.Wage, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetSubmitSectionState(vm => vm.Wage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetSubmitSectionState(vm => vm.Wage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetSubmitSectionState(vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetSubmitSectionState(vm => vm.Programme, vm => vm.TrainingLevel);
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
            viewModel.TrainingSectionState = GetSubmitSectionState(vm => vm.Programme, vm => vm.TrainingType, vm => vm.TrainingTitle);
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