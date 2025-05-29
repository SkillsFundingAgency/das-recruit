using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
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

public class VacancyHowWillTheApprenticeTrainOrchestrator : VacancyValidatingOrchestrator<VacancyHowTheApprenticeWillTrainEditModel>
{
    private readonly IRecruitVacancyClient _vacancyClient;
    private readonly IReviewSummaryService _reviewSummaryService;
    private readonly IUtility _utility;
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.TrainingDescription | VacancyRuleSet.AdditionalTrainingDescription;

    public VacancyHowWillTheApprenticeTrainOrchestrator(IRecruitVacancyClient vacancyClient, IReviewSummaryService reviewSummaryService, IUtility utility,ILogger<VacancyHowWillTheApprenticeTrainOrchestrator> logger) : base(logger)
    {
        _vacancyClient = vacancyClient;
        _reviewSummaryService = reviewSummaryService;
        _utility = utility;
    }

    public async Task<VacancyHowTheApprenticeWillTrainModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.VacancyHowTheApprenticeWillTrain_Index_Get);
        var vm = new VacancyHowTheApprenticeWillTrainModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            TrainingDescription = vacancy.TrainingDescription,
            AdditionalTrainingDescription = vacancy.AdditionalTrainingDescription,
            IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy),
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(
                vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetTrainingDescriptionFieldIndicators());
        }
            
        vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);
            
        return vm;
    }
    
    public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyHowTheApprenticeWillTrainEditModel m, VacancyUser user)
    {
        var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.VacancyHowTheApprenticeWillTrain_Index_Post);

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
            v => _vacancyClient.Validate(v, ValidationRules),
            v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
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