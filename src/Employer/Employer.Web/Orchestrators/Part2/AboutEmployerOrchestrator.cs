using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class AboutEmployerOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<AboutEmployerOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<AboutEmployerEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerDescription | VacancyRuleSet.EmployerWebsiteUrl;

    public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(TaskListViewModel model)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model);
        var vm = new AboutEmployerViewModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            EmployerDescription =  await vacancyClient.GetEmployerDescriptionAsync(vacancy),
            EmployerTitle = await GetEmployerTitleAsync(vacancy),
            EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
            IsAnonymous = vacancy.IsAnonymous,
            IsDisabilityConfident = vacancy.IsDisabilityConfident,
            IsTaskList = model.IsTaskList,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy) && !model.IsTaskList,
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetAboutEmployerFieldIndicators());
        }

        return vm;
    }

    public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(AboutEmployerEditModel m)
    {
        var vm = await GetAboutEmployerViewModelAsync((TaskListViewModel)m);
        vm.EmployerDescription = m.EmployerDescription;
        vm.EmployerWebsiteUrl = m.EmployerWebsiteUrl;
        vm.IsDisabilityConfident = m.IsDisabilityConfident;

        return vm;
    }

    public async Task<OrchestratorResponse> PostAboutEmployerEditModelAsync(AboutEmployerEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.EmployerDescription,
            FieldIdResolver.ToFieldId(v => v.EmployerDescription),
            vacancy,
            (v) => { return v.EmployerDescription = m.EmployerDescription; });

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.EmployerWebsiteUrl,
            FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl),
            vacancy,
            (v) => { return v.EmployerWebsiteUrl = m.EmployerWebsiteUrl; });
            
        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.DisabilityConfident,
            FieldIdResolver.ToFieldId(v => v.DisabilityConfident),
            vacancy,
            (v) =>
            {
                return v.DisabilityConfident = m.IsDisabilityConfident ? DisabilityConfident.Yes : DisabilityConfident.No;
            });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            async v =>    
            {
                await vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                await UpdateEmployerProfileAsync(vacancy, m.EmployerDescription, user);
            }
        );
    }

    private async Task UpdateEmployerProfileAsync(Vacancy vacancy, string employerDescription, VacancyUser user)
    {
        var employerProfile =
            await vacancyClient.GetEmployerProfileAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);

        if (employerProfile == null)
        {
            throw new NullReferenceException($"No Employer Profile was found for employerAccount: {vacancy.EmployerAccountId}, " +
                                             $"accountLegalEntityPublicHashedId : {vacancy.AccountLegalEntityPublicHashedId}");
        }

        if (employerProfile.AboutOrganisation != employerDescription)
        {
            employerProfile.AboutOrganisation = employerDescription;
            await vacancyClient.UpdateEmployerProfileAsync(employerProfile, user);
        }
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, AboutEmployerEditModel>();

        mappings.Add(e => e.EmployerDescription, vm => vm.EmployerDescription);
        mappings.Add(e => e.EmployerWebsiteUrl, vm => vm.EmployerWebsiteUrl);

        return mappings;
    }

    private async Task<string> GetEmployerTitleAsync(Vacancy vacancy)
    {
        if (vacancy.IsAnonymous)
            return vacancy.LegalEntityName;

        return await vacancyClient.GetEmployerNameAsync(vacancy);
    }
}