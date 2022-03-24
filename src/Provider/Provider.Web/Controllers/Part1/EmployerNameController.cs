using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.FeatureToggle;
using Esfa.Recruit.Shared.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    [Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
    public class EmployerNameController : EmployerControllerBase
    {
        private EmployerNameOrchestrator _orchestrator;
        private readonly IFeature _feature;

        public EmployerNameController(EmployerNameOrchestrator orchestrator,
            IHostingEnvironment hostingEnvironment, IFeature feature) : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
            _feature = feature;
        } 

        [HttpGet("employer-name", Name = RouteNames.EmployerName_Get)]
        public async Task<IActionResult> EmployerName(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var employerInfoModel = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());            
            //if matching cookie is not found redirect to legal entity selection
            //this could happen if the user navigates straight to employer-name end point
            //by passing employer or location end point
            if (employerInfoModel == null && !_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList)) 
                return RedirectToRoute(RouteNames.Employer_Get);
            var vm = await _orchestrator.GetEmployerNameViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("employer-name", Name = RouteNames.EmployerName_Post)]
        public async Task<IActionResult> EmployerName(EmployerNameEditModel model, [FromQuery] bool wizard)
        { 
            var employerInfoModel = GetVacancyEmployerInfoCookie(model.VacancyId.GetValueOrDefault());
            //respective cookie can go missing if user has opened another vacancy in a different browser tab 
            if(employerInfoModel == null && !_feature.IsFeatureEnabled((FeatureNames.ProviderTaskList)))
                return RedirectToRoute(RouteNames.Employer_Get);

            if(_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                employerInfoModel = new VacancyEmployerInfoModel
                {
                    VacancyId = model.VacancyId
                };
            }
            
            var response = await _orchestrator.PostEmployerNameEditModelAsync(model, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerNameViewModelAsync(model, employerInfoModel, User.ToVacancyUser());
                vm.PageInfo.SetWizard(wizard);
                vm.NewTradingName = model.NewTradingName;
                vm.SelectedEmployerIdentityOption = model.SelectedEmployerIdentityOption;
                vm.AnonymousName = model.AnonymousName;
                vm.AnonymousReason = model.AnonymousReason;
                return View(vm);
            }

            employerInfoModel.EmployerIdentityOption = model.SelectedEmployerIdentityOption;
            employerInfoModel.NewTradingName = model.SelectedEmployerIdentityOption == EmployerIdentityOption.NewTradingName ? model.NewTradingName : null;
            employerInfoModel.AnonymousName = model.SelectedEmployerIdentityOption == EmployerIdentityOption.Anonymous ? model.AnonymousName : null;
            employerInfoModel.AnonymousReason = model.SelectedEmployerIdentityOption == EmployerIdentityOption.Anonymous ? model.AnonymousReason : null;
            SetVacancyEmployerInfoCookie(employerInfoModel);

            if (_feature.IsFeatureEnabled(FeatureNames.ProviderTaskList))
            {
                return RedirectToRoute(RouteNames.AboutEmployer_Get, new {model.Ukprn, model.VacancyId});
            }
            
            return RedirectToRoute(RouteNames.Location_Get, new {Wizard = wizard, model.Ukprn, model.VacancyId});
        }

        [HttpGet("employer-name-cancel", Name = RouteNames.EmployerName_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard, vrm);
        }
    }
}