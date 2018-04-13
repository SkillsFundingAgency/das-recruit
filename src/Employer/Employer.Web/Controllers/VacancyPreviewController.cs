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
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

            return View(viewModel);
        }

        [HttpPost("preview", Name = RouteNames.Preview_Submit_Post)]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var response = await _orchestrator.TrySubmitVacancyAsync(m);

            if (!response.Success)
            {
                FlattenErrors(response);
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

            viewModel.ShortDescriptionSectionState = GetSubmitSectionState(viewModel, vm => vm.ShortDescription);
            viewModel.ClosingDateSectionState = GetSubmitSectionState(viewModel, vm => vm.ClosingDate);
            viewModel.WorkingWeekSectionState = GetSubmitSectionState(viewModel, vm => vm.Wage, vm => vm.HoursPerWeek, vm => vm.WorkingWeekDescription);
            viewModel.WageTextSectionState = GetSubmitSectionState(viewModel, vm => vm.Wage, vm => vm.PossibleStartDate);
            viewModel.ExpectedDurationSectionState = GetSubmitSectionState(viewModel, vm => vm.Wage, vm => vm.ExpectedDuration);
            viewModel.PossibleStartDateSectionState = GetSubmitSectionState(viewModel, vm => vm.PossibleStartDate);
            viewModel.TrainingLevelSectionState = GetSubmitSectionState(viewModel, vm => vm.Programme, vm => vm.TrainingLevel);
            viewModel.NumberOfPositionsSectionState = GetSubmitSectionState(viewModel, vm => vm.NumberOfPositions);
            viewModel.DescriptionsSectionState = GetSubmitSectionState(viewModel, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetSubmitSectionState(viewModel, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetSubmitSectionState(viewModel, vm => vm.Qualifications);
            viewModel.ThingsToConsiderSectionState = GetSubmitSectionState(viewModel, vm => vm.ThingsToConsider);
            viewModel.EmployerNameSectionState = GetSubmitSectionState(viewModel, vm => vm.EmployerName);
            viewModel.EmployerDescriptionSectionState = GetSubmitSectionState(viewModel, vm => vm.EmployerDescription);
            viewModel.EmployerWebsiteUrlSectionState = GetSubmitSectionState(viewModel, vm => vm.EmployerWebsiteUrl);
            viewModel.ContactSectionState = GetSubmitSectionState(viewModel, vm => vm.ContactName, vm => vm.ContactEmail, vm => vm.ContactTelephone);
            viewModel.EmployerAddressSectionState = GetSubmitSectionState(viewModel, vm => vm.EmployerAddressElements);
            viewModel.ApplicationInstructionsSectionState = GetSubmitSectionState(viewModel, vm => vm.ApplicationInstructions);
            viewModel.ApplicationUrlSectionState = GetSubmitSectionState(viewModel, vm => vm.ApplicationUrl);
            viewModel.ProviderSectionState = GetSubmitSectionState(viewModel, vm => vm.Provider, vm => vm.ProviderName);
            viewModel.TrainingSectionState = GetSubmitSectionState(viewModel, vm => vm.Programme, vm => vm.TrainingType, vm => vm.TrainingTitle);

            return View("VacancyPreview", viewModel);
        }
        
        private VacancyPreviewSectionState GetViewSectionState<T>(T vm, params Expression<Func<T, object>>[] properties)
        {
            foreach (var property in properties)
            {
                var propertyValueFunc = property.Compile();
                var propertyValue = propertyValueFunc(vm);

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
                    foreach (var item in listProperty)
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

        private VacancyPreviewSectionState GetSubmitSectionState<T>(T vm, params Expression<Func<T, object>>[] properties)
        {
            foreach (var property in properties)
            {
                var propertyName = property.GetPropertyName();
                if (ModelState.Keys.Where(k => k == propertyName).Any(k => ModelState[k].Errors.Any()))
                {
                    return VacancyPreviewSectionState.Invalid;
                }
            }
            
            return VacancyPreviewSectionState.Valid;
        }
        
        private void FlattenErrors(OrchestratorResponse response)
        {
            //Flatten errors to their parent instead. 'Qualifications[1].Grade' > 'Qualifications'
            foreach (var error in response.Errors.Errors)
            {
                var start = error.PropertyName.IndexOf('[');
                if (start > -1)
                {
                    error.PropertyName = error.PropertyName.Substring(0, start);
                }
            }
        }
    }
}