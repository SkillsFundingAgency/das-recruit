using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
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

public class VacancyWorkDescriptionOrchestrator : VacancyValidatingOrchestrator<VacancyWorkDescriptionEditModel>
{
    private readonly IRecruitVacancyClient _vacancyClient;
    private readonly IReviewSummaryService _reviewSummaryService;
    private readonly IUtility _utility;
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.Description;
    public VacancyWorkDescriptionOrchestrator(IRecruitVacancyClient vacancyClient, IReviewSummaryService reviewSummaryService,IUtility utility,ILogger<VacancyWorkDescriptionOrchestrator> logger) : base(logger)
    {
        _vacancyClient = vacancyClient;
        _reviewSummaryService = reviewSummaryService;
        _utility = utility;
    }

    public async Task<VacancyWorkDescriptionModel> GetVacancyDescriptionViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.VacancyWorkDescription_Index_Get);
        var vm = new VacancyWorkDescriptionModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vacancy.EmployerAccountId,
            Title = vacancy.Title,
            VacancyDescription = vacancy.Description,
            IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy),
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(
                vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetWhatWillTheApprenticeDoAtWorkDescriptionFieldIndicators());
        }
            
        vm.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);
            
        return vm;
    }

    public async Task<OrchestratorResponse> PostVacancyDescriptionEditModelAsync(VacancyWorkDescriptionEditModel m, VacancyUser user)
    {
        var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.VacancyWorkDescription_Index_Post);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.Description,
            FieldIdResolver.ToFieldId(v => v.Description),
            vacancy,
            (v) => { return v.Description = m.VacancyDescription; });

        return await ValidateAndExecute(
            vacancy,
            v => _vacancyClient.Validate(v, ValidationRules),
            v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, user)
        );
    }


    protected override EntityToViewModelPropertyMappings<Vacancy, VacancyWorkDescriptionEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, VacancyWorkDescriptionEditModel>();

        mappings.Add(e => e.Description, vm => vm.VacancyDescription);

        return mappings;
    }
}