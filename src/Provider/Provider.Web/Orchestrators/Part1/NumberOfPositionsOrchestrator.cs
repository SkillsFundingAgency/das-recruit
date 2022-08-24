using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class NumberOfPositionsOrchestrator : VacancyValidatingOrchestrator<NumberOfPositionsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public NumberOfPositionsOrchestrator(IRecruitVacancyClient recruitVacancyClient, 
            ILogger<NumberOfPositionsOrchestrator> logger, IReviewSummaryService reviewSummaryService, IUtility utility) : base(logger)
        {
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelForExistingVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.NumberOfPositions_Get);

            var vm = new NumberOfPositionsViewModel
            {
                Title = vacancy.Title,
                VacancyId = vacancy.Id,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                Ukprn = vacancy.TrainingProvider.Ukprn.GetValueOrDefault()
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetNumberOfPositionsFieldIndicators());
            }

            return vm;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelFromEditModelAsync(NumberOfPositionsEditModel model)
        {
            var vm = await GetNumberOfPositionsViewModelForExistingVacancyAsync(model);
            vm.NumberOfPositions = model.NumberOfPositions;
            return vm;
        }

        public async Task<OrchestratorResponse<Guid>> PostNumberOfPositionsEditModelAsync(NumberOfPositionsEditModel model, VacancyUser user)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(model, RouteNames.NumberOfPositions_Post);
            
            SetVacancyWithProviderReviewFieldIndicators(
               vacancy.NumberOfPositions,
               FieldIdResolver.ToFieldId(v => v.NumberOfPositions),
               vacancy,
               (v) =>
               {
                   return v.NumberOfPositions = int.TryParse(model.NumberOfPositions, out var n) ? n : default(int?);
               });

            return await ValidateAndExecute(
                    vacancy,
                    v => _recruitVacancyClient.Validate(v, ValidationRules),
                    async v =>
                    {
                        await _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, user);
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
