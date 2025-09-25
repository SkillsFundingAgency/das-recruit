using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Employer.Web.Orchestrators.Part1;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Controllers.Part1;

[Route(RoutePaths.AccountVacancyRoutePath)]
public class EmployerNameController(EmployerNameOrchestrator orchestrator, IWebHostEnvironment hostingEnvironment)
    : EmployerControllerBase(hostingEnvironment)
{
    [HttpGet("employer-name", Name = RouteNames.EmployerName_Get)]
    public async Task<IActionResult> EmployerName(TaskListViewModel vrm)
    {
        var employerInfoModel = GetVacancyEmployerInfoCookie(vrm.VacancyId);            
        //if matching cookie is not found redirect to legal entity selection
        //this could happen if the user navigates straight to employer-name end point
        //by passing employer or location end point
            
        var vm = await orchestrator.GetEmployerNameViewModelAsync(vrm, employerInfoModel);
        if (vm == null)
        {
            return RedirectToRoute(RouteNames.Employer_Get, new {vrm.VacancyId, vrm.EmployerAccountId});
        }

        return View(vm);
    }

    [HttpPost("employer-name", Name = RouteNames.EmployerName_Post)]
    public async Task<IActionResult> EmployerName(EmployerNameEditModel model)
    {
        var employerInfoModel = new VacancyEmployerInfoModel
        {
            VacancyId = model.VacancyId,
            EmployerAccountId = model.EmployerAccountId
        };

        var response = await orchestrator.PostEmployerNameEditModelAsync(model, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        if (!ModelState.IsValid)
        {
            var vm = await orchestrator.GetEmployerNameViewModelAsync(model, employerInfoModel);
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

        return RedirectToRoute(RouteNames.LegalEntityAgreement_SoftStop_Get, new {model.VacancyId, model.EmployerAccountId, wizard = model.IsTaskList});
    }

    [HttpGet("employer-name-cancel", Name = RouteNames.EmployerName_Cancel)]
    public IActionResult Cancel(TaskListViewModel model)
    {
        return CancelAndRedirect(model.IsTaskList, model);
    }
}