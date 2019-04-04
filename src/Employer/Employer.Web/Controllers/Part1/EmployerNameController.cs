using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1
{
    [Route(RoutePaths.AccountVacancyRoutePath)]
    public class EmployerNameController : EmployerControllerBase
    {
        private EmployerNameOrchestrator _orchestrator;
        public EmployerNameController(EmployerNameOrchestrator orchestrator,
            IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            _orchestrator = orchestrator;
        }

        [HttpGet("employer-name", Name = RouteNames.EmployerName_Get)]
        public async Task<IActionResult> EmployerName(VacancyRouteModel vrm, [FromQuery] string wizard = "true")
        {
            var employerInfoModel = GetVacancyEmployerInfoCookie(vrm.VacancyId);            
            //if matching cookie is not found redirect to legal entity selection
            //this could happen if the user navigates straight to employer-name end point
            //by passing employer or location end point
            if (employerInfoModel == null) 
                return RedirectToRoute(RouteNames.Employer_Get);
            var vm = await _orchestrator.GetEmployerNameViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("employer-name", Name = RouteNames.EmployerName_Post)]
        public async Task<IActionResult> EmployerName(EmployerNameEditModel model, [FromQuery] bool wizard)
        { 
            var employerInfoModel = GetVacancyEmployerInfoCookie(model.VacancyId);
            //respective cookie can go missing if user has opened another vacancy in a different browser tab 
            if(employerInfoModel == null)
                return RedirectToRoute(RouteNames.Employer_Get);

            var response = await _orchestrator.PostEmployerNameEditModelAsync(model, employerInfoModel, User.ToVacancyUser());

            if (!response.Success)
            {
                response.AddErrorsToModelState(ModelState);
            }

            if (!ModelState.IsValid)
            {
                var vm = await _orchestrator.GetEmployerNameViewModelAsync(model, employerInfoModel, User.ToVacancyUser());
                vm.PageInfo.SetWizard(wizard);
                vm.NewTradingName = model.NewTradingName;
                vm.SelectedEmployerNameOption = model.SelectedEmployerNameOption;
                vm.AnonymousName = model.AnonymousName;
                vm.AnonymousReason = model.AnonymousReason;
                return View(vm);
            }

            employerInfoModel.EmployerNameOption = model.SelectedEmployerNameOption;
            employerInfoModel.NewTradingName = model.NewTradingName;
            employerInfoModel.AnonymousName = model.AnonymousName;
            employerInfoModel.AnonymousReason = model.AnonymousReason;
            SetVacancyEmployerInfoCookie(employerInfoModel);

            return RedirectToRoute(RouteNames.LegalEntityAgreement_SoftStop_Get, new {Wizard = wizard});
        }

        [HttpGet("employer-name-cancel", Name = RouteNames.EmployerName_Cancel)]
        public IActionResult Cancel(VacancyRouteModel vrm, [FromQuery] bool wizard)
        {
            return CancelAndRedirect(wizard);
        }
    }
}