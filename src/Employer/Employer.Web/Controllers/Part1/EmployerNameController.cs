using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
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
            var employerInfoModel = GetEmployerInfoCookie(vrm.VacancyId);
            var vm = await _orchestrator.GetEmployerNameViewModelAsync(vrm, employerInfoModel, User.ToVacancyUser());
            vm.PageInfo.SetWizard(wizard);
            return View(vm);
        }

        [HttpPost("employer-name", Name = RouteNames.EmployerName_Post)]
        public async Task<IActionResult> EmployerName(EmployerNameEditModel model, [FromQuery] bool wizard)
        { 
            var employerInfoModel = GetEmployerInfoCookie(model.VacancyId);
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
                return View(vm);
            }

            employerInfoModel.EmployerNameOption = model.SelectedEmployerNameOption;
            SetEmployerInfoCookie(model.VacancyId, employerInfoModel);
            return RedirectToRoute(RouteNames.LegalEntityAgreement_SoftStop_Get);
        }
    }
}