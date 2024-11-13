using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
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

        public EmployerNameController(EmployerNameOrchestrator orchestrator,
            IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
        } 

        [HttpGet("employer-name", Name = RouteNames.EmployerName_Get)]
        public async Task<IActionResult> EmployerName(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var employerInfoModel = GetVacancyEmployerInfoCookie(vrm.VacancyId.GetValueOrDefault());            
            //if matching cookie is not found redirect to legal entity selection
            //this could happen if the user navigates straight to employer-name end point
            //by passing employer or location end point
            var vm = await _orchestrator.GetEmployerNameViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("employer-name", Name = RouteNames.EmployerName_Post)]
        public async Task<IActionResult> EmployerName(EmployerNameEditModel model, [FromQuery] bool wizard)
        { 
            var employerInfoModel = GetVacancyEmployerInfoCookie(model.VacancyId.GetValueOrDefault());
            //respective cookie can go missing if user has opened another vacancy in a different browser tab 

            employerInfoModel = new VacancyEmployerInfoModel
            {
                VacancyId = model.VacancyId
            };
            
            var response = await _orchestrator.PostEmployerNameEditModelAsync(model, User.ToVacancyUser());
            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            var vm = await _orchestrator.GetEmployerNameViewModelAsync(model, employerInfoModel, User.ToVacancyUser());
            if (!ModelState.IsValid)
            {
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

            if (!vm.IsTaskListCompleted)
            {
                return RedirectToRoute(RouteNames.AboutEmployer_Get, new {model.Ukprn, model.VacancyId});
            }
            return RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new {model.Ukprn, model.VacancyId});
        }

        [HttpGet("employer-name-cancel", Name = RouteNames.EmployerName_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard, vrm);
        }
    }
}