using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1;

public class NumberOfPositionsOrchestrator(
    IRecruitVacancyClient vacancyClient,
    ILogger<NumberOfPositionsOrchestrator> logger,
    IReviewSummaryService reviewSummaryService,
    IUtility utility)
    : VacancyValidatingOrchestrator<NumberOfPositionsEditModel>(logger)
{
    private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;

    public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelAsync(VacancyRouteModel vrm)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm);

        var vm = new NumberOfPositionsViewModel
        {
            VacancyId = vacancy.Id,
            EmployerAccountId = vrm.EmployerAccountId,
            NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
            PageInfo = utility.GetPartOnePageInfo(vacancy),
            VacancyTitle = vacancy.Title,
        };

        if (vacancy.Status == VacancyStatus.Referred)
        {
            vm.Review = await reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                ReviewFieldMappingLookups.GetNumberOfPositionsFieldIndicators());
        }

        return vm;
    }

    public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelAsync(NumberOfPositionsEditModel m)
    {
        var vm = await GetNumberOfPositionsViewModelAsync((VacancyRouteModel)m);
        vm.NumberOfPositions = m.NumberOfPositions;
        return vm;
    }


    public async Task<OrchestratorResponse<Guid>> PostNumberOfPositionsEditModelAsync(NumberOfPositionsEditModel m, VacancyUser user)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(m);

        SetVacancyWithEmployerReviewFieldIndicators(
            vacancy.NumberOfPositions,
            FieldIdResolver.ToFieldId(v => v.NumberOfPositions),
            vacancy,
            (v) =>
            {
                return v.NumberOfPositions = int.TryParse(m.NumberOfPositions, out var n) ? n : null;
            });
 
        return await ValidateAndExecute(
            vacancy, 
            v => vacancyClient.Validate(v, ValidationRules),
            async v =>
            {
                await vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
                return v.Id;
            }
        );
    }

    protected override EntityToViewModelPropertyMappings<Vacancy, NumberOfPositionsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<Vacancy, NumberOfPositionsEditModel>();
        mappings.Add(e => e.NumberOfPositions, vm => vm.NumberOfPositions);
        return mappings;
    }
}