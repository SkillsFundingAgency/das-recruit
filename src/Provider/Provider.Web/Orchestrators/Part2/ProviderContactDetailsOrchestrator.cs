using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2;

public class ProviderContactDetailsOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<ProviderContactDetailsOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<ProviderContactDetailsEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.ProviderContactDetails;

    public async Task<ProviderContactDetailsViewModel> GetProviderContactDetailsViewModelAsync(TaskListViewModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.ProviderContactDetails_Get);

        var vm = new ProviderContactDetailsViewModel
        {
            AddContactDetails = !string.IsNullOrEmpty(vacancy.ProviderContact?.Name) || 
                                !string.IsNullOrEmpty(vacancy.ProviderContact?.Email) ||
                                !string.IsNullOrEmpty(vacancy.ProviderContact?.Phone) ? true : (bool?) null,
            IsTaskList = vrm.IsTaskList,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy) && !vrm.IsTaskList,
            ProviderContactEmail = vacancy.ProviderContact?.Email,
            ProviderContactName = vacancy.ProviderContact?.Name,
            ProviderContactPhone = vacancy.ProviderContact?.Phone,
            ProviderName = vacancy.TrainingProvider?.Name,
            Title = vacancy.Title,
            Ukprn = vrm.Ukprn,
            VacancyId = vrm.VacancyId,
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetProviderContactDetailsFieldIndicators());
        }

        return vm;
    }

    public async Task<ProviderContactDetailsViewModel> GetProviderContactDetailsViewModelAsync(ProviderContactDetailsEditModel m)
    {
        var vm = await GetProviderContactDetailsViewModelAsync((TaskListViewModel)m);
        vm.ProviderContactName = m.ProviderContactName;
        vm.ProviderContactEmail = m.ProviderContactEmail;
        vm.ProviderContactPhone = m.ProviderContactPhone;

        return vm;
    }

    public async Task<OrchestratorResponse> PostProviderContactDetailsEditModelAsync(ProviderContactDetailsEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.ProviderContactDetails_Post);
        vacancy.HasChosenProviderContactDetails = true;
        if (vacancy.ProviderContact == null)
        {
            vacancy.ProviderContact = new ContactDetail();
        }
                

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.ProviderContact.Name,
            FieldIdResolver.ToFieldId(v => v.ProviderContact.Name),
            vacancy,
            (v) => { return v.ProviderContact.Name = m.ProviderContactName; });

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.ProviderContact.Email,
            FieldIdResolver.ToFieldId(v => v.ProviderContact.Email),
            vacancy,
            (v) => { return v.ProviderContact.Email = m.ProviderContactEmail; });

        SetVacancyWithProviderReviewFieldIndicators(
            vacancy.ProviderContact.Phone,
            FieldIdResolver.ToFieldId(v => v.ProviderContact.Phone),
            vacancy,
            (v) => { return v.ProviderContact.Phone = m.ProviderContactPhone; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, ProviderContactDetailsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, ProviderContactDetailsEditModel>();

        mappings.Add(e => e.ProviderContact.Name, vm => vm.ProviderContactName);
        mappings.Add(e => e.ProviderContact.Email, vm => vm.ProviderContactEmail);
        mappings.Add(e => e.ProviderContact.Phone, vm => vm.ProviderContactPhone);

        return mappings;
    }
}