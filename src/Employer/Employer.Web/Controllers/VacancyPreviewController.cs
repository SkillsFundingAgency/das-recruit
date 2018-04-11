using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.Preview;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

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

        [HttpGet("vacancy-preview", Name = RouteNames.Vacancy_Preview_Get)]
        public async Task<IActionResult> VacancyPreview(Guid vacancyId)
        {
            var vm = await GetViewModel(vacancyId);
            
            return View(vm);
        }

        [HttpPost("vacancy-submit", Name = RouteNames.Preview_Submit_Post)]
        public async Task<IActionResult> Submit(SubmitEditModel m)
        {
            var response = await _orchestrator.TrySubmitVacancyAsync(m);

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

            var vm = await GetViewModel(m.VacancyId);
            
            return View("VacancyPreview", vm);
        }

        private async Task<VacancyPreviewViewModel> GetViewModel(Guid vacancyId)
        {
            var vm = await _orchestrator.GetVacancyPreviewViewModelAsync(vacancyId);

            vm.DescriptionSectionState = GetDescriptionSectionState(vm);
            vm.SkillsSectionState = GetSkillsSectionState(vm);
            vm.QualificationsSectionState = GetQualificationsSectionState(vm);

            return vm;
        }

        private VacancyPreviewSectionState GetDescriptionSectionState(VacancyPreviewViewModel vm)
        {
            if (IsModelStateInvalidForProperties(nameof(vm.VacancyDescription), nameof(vm.TrainingDescription), nameof(vm.OutcomeDescription)))
            {
                return VacancyPreviewSectionState.Invalid;
            }

            if (string.IsNullOrWhiteSpace(vm.VacancyDescription) || string.IsNullOrWhiteSpace(vm.TrainingDescription) || string.IsNullOrWhiteSpace(vm.OutcomeDescription))
            {
                return VacancyPreviewSectionState.Incomplete;
            }

            return VacancyPreviewSectionState.Valid;
        }

        private VacancyPreviewSectionState GetSkillsSectionState(VacancyPreviewViewModel vm)
        {
            if (IsModelStateInvalidForProperties(nameof(vm.Skills)))
            {
                return VacancyPreviewSectionState.Invalid;
            }

            if (!vm.Skills.Any())
            {
                return VacancyPreviewSectionState.Incomplete;
            }

            return VacancyPreviewSectionState.Valid;
        }

        private VacancyPreviewSectionState GetQualificationsSectionState(VacancyPreviewViewModel vm)
        {
            if (IsModelStateInvalidForProperties(nameof(vm.Qualifications)))
            {
                return VacancyPreviewSectionState.Invalid;
            }

            if (!vm.Qualifications.Any())
            {
                return VacancyPreviewSectionState.Incomplete;
            }

            return VacancyPreviewSectionState.Valid;
        }

        private bool IsModelStateInvalidForProperties(params string[] propertyNames)
        {
            return ModelState.Keys.Where(propertyNames.Contains).Any(k => ModelState[k].Errors.Any());
        }

        private void FlattenModelState()
        {
            //Change 'Qualification[1].Grade' to 'Qualification'
            
        }
    }
}