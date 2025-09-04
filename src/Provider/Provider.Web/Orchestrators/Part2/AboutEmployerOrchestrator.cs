using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2;

public class AboutEmployerOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<AboutEmployerOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<AboutEmployerEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerDescription | VacancyRuleSet.EmployerWebsiteUrl;

    public async Task<AboutEmployerViewModel> GetAboutEmployerViewModelAsync(TaskListViewModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.AboutEmployer_Get);

        var vm = new AboutEmployerViewModel
        {
            EmployerDescription = vacancy.EmployerDescription,
            EmployerTitle = await GetEmployerTitleAsync(vacancy),
            EmployerWebsiteUrl = vacancy.EmployerWebsiteUrl,
            IsAnonymous = vacancy.IsAnonymous,
            IsDisabilityConfident = vacancy.IsDisabilityConfident,
            IsTaskList = vrm.IsTaskList,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy) && !vrm.IsTaskList,
            Title = vacancy.Title,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId,
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
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.AboutEmployer_Post);

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.EmployerDescription,
            FieldIdResolver.ToFieldId(v => v.EmployerDescription),
            vacancy,
            (v) => { return v.EmployerDescription = m.EmployerDescription; });

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.EmployerWebsiteUrl,
            FieldIdResolver.ToFieldId(v => v.EmployerWebsiteUrl),
            vacancy,
            (v) => { return v.EmployerWebsiteUrl = m.EmployerWebsiteUrl; });

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.DisabilityConfident,
            FieldIdResolver.ToFieldId(v => v.DisabilityConfident),
            vacancy,
            (v) => { return v.DisabilityConfident = m.IsDisabilityConfident ? DisabilityConfident.Yes : DisabilityConfident.No; });
            
        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            async v =>    
            {
                await vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
            }
        );
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