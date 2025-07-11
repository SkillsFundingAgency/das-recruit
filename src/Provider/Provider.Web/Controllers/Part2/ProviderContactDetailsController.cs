﻿using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Extensions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part2;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Shared.Web.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Controllers.Part2;

[Route(RoutePaths.AccountVacancyRoutePath)]
[Authorize(Policy = nameof(PolicyNames.HasContributorOrAbovePermission))]
public class ProviderContactDetailsController(ProviderContactDetailsOrchestrator orchestrator) : Controller
{
    [HttpGet("provider-contact-details", Name = RouteNames.ProviderContactDetails_Get)]
    public async Task<IActionResult> ProviderContactDetails(TaskListViewModel model)
    {
        var vm = await orchestrator.GetProviderContactDetailsViewModelAsync(model);
        return View(vm);
    }

    [HttpPost("provider-contact-details", Name = RouteNames.ProviderContactDetails_Post)]
    public async Task<IActionResult> ProviderContactDetails(ProviderContactDetailsEditModel m)
    {      
        if (!ModelState.IsValid)
        {
            var viewModel = await orchestrator.GetProviderContactDetailsViewModelAsync(m);    
            return View(viewModel);
        }

        if (m.AddContactDetails.GetValueOrDefault())
        {
            if (string.IsNullOrEmpty(m.ProviderContactEmail) 
                && string.IsNullOrEmpty(m.ProviderContactName) 
                && string.IsNullOrEmpty(m.ProviderContactPhone))
            {
                ModelState.AddModelError(nameof(m.AddContactDetails), "Enter contact details");
                var viewModel = await orchestrator.GetProviderContactDetailsViewModelAsync(m);    
                return View(viewModel);
            }
        }
        else
        {
            m.ProviderContactEmail = null;
            m.ProviderContactName = null;
            m.ProviderContactPhone = null;
        }
            
        var response = await orchestrator.PostProviderContactDetailsEditModelAsync(m, User.ToVacancyUser());

        if (!response.Success)
        {
            response.AddErrorsToModelState(ModelState);
        }

        var vm = await orchestrator.GetProviderContactDetailsViewModelAsync(m);
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        return vm.IsTaskListCompleted
            ? RedirectToRoute(RouteNames.ProviderCheckYourAnswersGet, new { m.VacancyId, m.Ukprn })
            : RedirectToRoute(RouteNames.ApplicationProcess_Get, new { m.VacancyId, m.Ukprn, wizard = m.IsTaskList });
    }
}