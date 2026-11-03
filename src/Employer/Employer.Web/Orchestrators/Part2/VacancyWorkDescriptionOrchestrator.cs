using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyWorkDescription;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class VacancyWorkDescriptionOrchestrator(
    IRecruitVacancyClient vacancyClient,
    IReviewSummaryService reviewSummaryService,
    IUtility utility,
    ILogger<VacancyWorkDescriptionOrchestrator> logger)
    : VacancyValidatingOrchestrator<VacancyWorkDescriptionEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.Description;

    public async Task<VacancyWorkDescriptionModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
        var vm = new VacancyWorkDescriptionModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            VacancyDescription = vacancy.Description,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy),
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(
                vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetWhatWillTheApprenticeDoAtWorkDescriptionFieldIndicators());
        }
            
        vm.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);
            
        return vm;
    }

    public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyWorkDescriptionEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.Description,
            FieldIdResolver.ToFieldId(v => v.Description),
            vacancy,
            (v) => { return v.Description = m.VacancyDescription; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }


    protected override EntityToViewModelPropertyMappings<Vacancy, VacancyWorkDescriptionEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyWorkDescriptionEditModel>
        {
            { e => e.Description, vm => vm.VacancyDescription }
        };

        return mappings;
    }
}