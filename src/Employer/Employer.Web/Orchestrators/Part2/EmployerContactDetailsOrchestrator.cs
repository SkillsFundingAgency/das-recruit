using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.EmployerContactDetails;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class EmployerContactDetailsOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<EmployerContactDetailsOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<EmployerContactDetailsEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.EmployerContactDetails;

    public async Task<EmployerContactDetailsViewModel> GetEmployerContactDetailsViewModelAsync(TaskListViewModel model)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.EmployerContactDetails_Get);
        var vm = new EmployerContactDetailsViewModel
        {
            EmployerAccountId = vacancy.EmployerAccountId,
            EmployerContactEmail = vacancy.EmployerContact?.Email,
            EmployerContactName = vacancy.EmployerContact?.Name,
            EmployerContactPhone = vacancy.EmployerContact?.Phone,
            EmployerTitle = await GetEmployerTitleAsync(vacancy),
            IsTaskList = model.IsTaskList,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy) && !model.IsTaskList,
            Title = vacancy.Title,
            VacancyId = vacancy.Id,
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetEmployerContactDetailsFieldIndicators());
        }
            
        return vm;
    }

    public async Task<EmployerContactDetailsViewModel> GetEmployerContactDetailsViewModelAsync(EmployerContactDetailsEditModel m)
    {
        var vm = await GetEmployerContactDetailsViewModelAsync((TaskListViewModel)m);
        vm.EmployerContactName = m.EmployerContactName;
        vm.EmployerContactEmail = m.EmployerContactEmail;
        vm.EmployerContactPhone = m.EmployerContactPhone;

        return vm;
    }

    public async Task<OrchestratorResponse> PostEmployerContactDetailsEditModelAsync(EmployerContactDetailsEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.EmployerContactDetails_Post);

        if (vacancy.EmployerContact == null)
            vacancy.EmployerContact = new ContactDetail();

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.EmployerContact.Name,
            FieldIdResolver.ToFieldId(v => v.EmployerContact.Name),
            vacancy,
            (v) => { return v.EmployerContact.Name = m.EmployerContactName; });

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.EmployerContact.Email,
            FieldIdResolver.ToFieldId(v => v.EmployerContact.Email),
            vacancy,
            (v) => { return v.EmployerContact.Email = m.EmployerContactEmail; });

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.EmployerContact.Phone,
            FieldIdResolver.ToFieldId(v => v.EmployerContact.Phone),
            vacancy,
            (v) => { return v.EmployerContact.Phone = m.EmployerContactPhone; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, EmployerContactDetailsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, EmployerContactDetailsEditModel>();

        mappings.Add(e => e.EmployerContact.Name, vm => vm.EmployerContactName);
        mappings.Add(e => e.EmployerContact.Email, vm => vm.EmployerContactEmail);
        mappings.Add(e => e.EmployerContact.Phone, vm => vm.EmployerContactPhone);

        return mappings;
    }

    private async Task<string> GetEmployerTitleAsync(Vacancy vacancy)
    {
        if (vacancy.IsAnonymous)
            return vacancy.LegalEntityName;

        return await vacancyClient.GetEmployerNameAsync(vacancy);
    }
}