using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyHowTheApprenticeWillTrain;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2;

public class VacancyHowWillTheApprenticeTrainOrchestrator(
    IRecruitVacancyClient vacancyClient,
    IReviewSummaryService reviewSummaryService,
    IUtility utility,
    ILogger<VacancyHowWillTheApprenticeTrainOrchestrator> logger)
    : VacancyValidatingOrchestrator<VacancyHowTheApprenticeWillTrainEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription;

    public async Task<VacancyHowTheApprenticeWillTrainModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);
        var vm = new VacancyHowTheApprenticeWillTrainModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            TrainingDescription = vacancy.TrainingDescription,
            AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription,
            IsTaskListCompleted = utility.IsTaskListCompleted(vacancy),
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(
                vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetTrainingDescriptionFieldIndicators());
        }
            
        vm.IsTaskListCompleted = utility.IsTaskListCompleted(vacancy);
            
        return vm;
    }
    
    public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyHowTheApprenticeWillTrainEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.TrainingDescription,
            FieldIdResolver.ToFieldId(v => v.TrainingDescription),
            vacancy,
            (v) => { return v.TrainingDescription = m.TrainingDescription; });
        
        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.AdditionalTrainingDescription,
            FieldIdResolver.ToFieldId(v => v.AdditionalTrainingDescription),
            vacancy,
            (v) => { return v.AdditionalTrainingDescription = m.AdditionalTrainingDescription; });

        return await ValidateAndExecute(
            vacancy,
            v => vacancyClient.Validate(v, ValidationRules),
            v => vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }
    
    protected override EntityToViewModelPropertyMappings<Vacancy, VacancyHowTheApprenticeWillTrainEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyHowTheApprenticeWillTrainEditModel>();

        mappings.Add(e => e.TrainingDescription, vm => vm.TrainingDescription);
        mappings.Add(e => e.AdditionalTrainingDescription, vm => vm.AdditionalTrainingDescription);

        return mappings;
    }
}