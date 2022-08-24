using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
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

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class NumberOfPositionsOrchestrator : VacancyValidatingOrchestrator<NumberOfPositionsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public NumberOfPositionsOrchestrator(IRecruitVacancyClient vacancyClient, ILogger<NumberOfPositionsOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility) 
            : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.NumberOfPositions_Get);

            var vm = new NumberOfPositionsViewModel
            {
                VacancyId = vacancy.Id,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                PageInfo = _utility.GetPartOnePageInfo(vacancy)
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
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
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(m, RouteNames.NumberOfPositions_Post);

            SetVacancyWithEmployerReviewFieldIndicators(
                vacancy.NumberOfPositions,
                FieldIdResolver.ToFieldId(v => v.NumberOfPositions),
                vacancy,
                (v) =>
                {
                    return v.NumberOfPositions = int.TryParse(m.NumberOfPositions, out var n) ? n : default(int?);
                });
 
            return await ValidateAndExecute(
                vacancy, 
                v => _vacancyClient.Validate(v, ValidationRules),
                async v =>
                {
                    await _vacancyClient.UpdateDraftVacancyAsync(vacancy, user);
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
}
