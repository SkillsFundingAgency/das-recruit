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

            var vm = await GetViewModel(m.VacancyId);
            
            return View("VacancyPreview", vm);
        }

        private async Task<VacancyPreviewViewModel> GetViewModel(Guid vacancyId)
        {
            var viewModel = await _orchestrator.GetVacancyPreviewViewModelAsync(vacancyId);

            viewModel.DescriptionSectionState = GetSectionState(viewModel, vm => vm.VacancyDescription, vm => vm.TrainingDescription, vm => vm.OutcomeDescription);
            viewModel.SkillsSectionState = GetSectionState(viewModel, vm => vm.Skills);
            viewModel.QualificationsSectionState = GetSectionState(viewModel, vm => vm.Qualifications);

            return viewModel;
        }

        private VacancyPreviewSectionState GetSectionState<T>(T vm, params Expression<Func<T, object>>[] properties)
        {
            foreach (var property in properties)
            {
                var propertyName = property.GetPropertyName();
                if (ModelState.Keys.Where(k => k == propertyName).Any(k => ModelState[k].Errors.Any()))
                {
                    return VacancyPreviewSectionState.Invalid;
                }
            }

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

                if(propertyValue is IEnumerable listProperty)
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