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
        public async Task<IActionResult> VacancyPreview(Guid vacancyId)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vacancyId);
            
            SetViewSectionStates(viewModel);

            return View(viewModel);
        }
        
        [HttpPost("preview", Name = RouteNames.Preview_Submit_Post)]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var response = await _orchestrator.TrySubmitVacancyAsync(m, User.GetDisplayName(), User.GetEmailAddress());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }
            
            if (ModelState.IsValid && response.Data)
            {
                return RedirectToRoute(RouteNames.Submitted_Index_Get);
            }

            if (ModelState.IsValid && !response.Data)
            {
                ModelState.AddModelError(string.Empty, "Vacancy has already been submitted");
            }

            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(m.VacancyId);

            SetSubmitSectionStates(viewModel);
            
            return View("VacancyPreview", viewModel);
        }

        private void SetViewSectionStates(VacancyPreviewViewModel viewModel)
        {
            viewModel.ShortDescriptionSectionState = GetViewSectionState(viewModel, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetViewSectionState(viewModel, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetViewSectionState(viewModel, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetViewSectionState(viewModel, vm => vm.Wage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetViewSectionState(viewModel, vm => vm.Wage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetViewSectionState(viewModel, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetViewSectionState(viewModel, vm => vm.Programme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetViewSectionState(viewModel, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetViewSectionState(viewModel, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetViewSectionState(viewModel, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetViewSectionState(viewModel, vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = VacancyPreviewSectionState.Valid;
            viewModel.EmployerNameSectionState = GetViewSectionState(viewModel, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetViewSectionState(viewModel, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = VacancyPreviewSectionState.Valid;
            viewModel.ContactSectionState = VacancyPreviewSectionState.Valid;
            viewModel.EmployerAddressSectionState = GetViewSectionState(viewModel, vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = VacancyPreviewSectionState.Valid;
            viewModel.ApplicationUrlSectionState = GetViewSectionState(viewModel, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetViewSectionState(viewModel, vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetViewSectionState(viewModel, vm => vm.TrainingType, vm => vm.TrainingTitle);
        }
        
        private VacancyPreviewSectionState GetViewSectionState<T>(T vm, params Func<T, object>[] requiredProperties)
        {
            foreach (var requiredPropertyFunc in requiredProperties)
            {
                var propertyValue = requiredPropertyFunc(vm);

                if (propertyValue is null)
                {
                    return VacancyPreviewSectionState.Incomplete;
                }

                if (propertyValue is string stringProperty)
                {
                    if (string.IsNullOrWhiteSpace(stringProperty))
                    {
                        return VacancyPreviewSectionState.Incomplete;
                    }
                }

                if (propertyValue is IEnumerable listProperty)
                {
                    bool any = false;
                    foreach (var unused in listProperty)
                    {
                        any = true;
                        break;
                    }
                    if (any == false)
                    {
                        return VacancyPreviewSectionState.Incomplete;
                    }
                }
            }

            return VacancyPreviewSectionState.Valid;
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