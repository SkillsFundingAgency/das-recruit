using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyDescription;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class VacancyDescriptionOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<VacancyDescriptionOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<VacancyDescriptionEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.Description | VacancyRuleSet.TrainingDescription;

    public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);

        var vm = new VacancyDescriptionViewModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            VacancyDescription = vacancy.Description,
            TrainingDescription = vacancy.TrainingDescription,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy)
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(
                vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetVacancyDescriptionFieldIndicators());
        }
            
        vm.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);
            
        return vm;
    }

    public async Task<VacancyDescriptionViewModel> GetVacancyDescriptionViewModelAsync(VacancyDescriptionEditModel m)
    {
        var vm = await GetVacancyDescriptionViewModelAsync((VacancyRouteModel)m);

        vm.VacancyDescription = m.VacancyDescription;
        vm.TrainingDescription = m.TrainingDescription;

        return vm;
    }

    public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyDescriptionEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.Description,
            FieldIdResolver.ToFieldId(v => v.Description),
            vacancy,
            (v) => { return v.Description = m.VacancyDescription; });

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.TrainingDescription,
            FieldIdResolver.ToFieldId(v => v.TrainingDescription),
            vacancy,
            (v) => { return v.TrainingDescription = m.TrainingDescription; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyDescriptionEditModel>();

        mappings.Add(e => e.Description, vm => vm.VacancyDescription);
        mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);

        return mappings;
    }
}