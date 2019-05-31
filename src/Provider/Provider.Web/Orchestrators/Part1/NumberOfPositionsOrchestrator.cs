using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class NumberOfPositionsOrchestrator : EntityValidatingOrchestrator<Vacancy, NumberOfPositionsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;
        private readonly IProviderVacancyClient _providerVacancyClient;
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public NumberOfPositionsOrchestrator(IProviderVacancyClient providerVacancyClient, IRecruitVacancyClient recruitVacancyClient, 
            ILogger<NumberOfPositionsOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _providerVacancyClient = providerVacancyClient;
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelForNewVacancyAsync(string employerAccountId, long ukprn)
        {
            await ValidateEmployerAccountIdAsync(ukprn, employerAccountId);

            var vm = new NumberOfPositionsViewModel
            {
                EmployerAccountId = employerAccountId,
                Ukprn = ukprn,
                PageInfo = new PartOnePageInfoViewModel()
            };
            return vm;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelForExistingVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.NumberOfPositions_Get);

            var vm = new NumberOfPositionsViewModel
            {
                    VacancyId = vacancy.Id,
                    NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                    PageInfo = Utility.GetPartOnePageInfo(vacancy),
                    Ukprn = vacancy.TrainingProvider.Ukprn.GetValueOrDefault(),
                    EmployerAccountId = vacancy.EmployerAccountId
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetNumberOfPositionsFieldIndicators());
            }

            return vm;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelFromEditModelAsync(VacancyRouteModel vrm, NumberOfPositionsEditModel model, long ukprn)
        {
            NumberOfPositionsViewModel vm;

            if(model.VacancyId.HasValue)
            {
                vm = await GetNumberOfPositionsViewModelForExistingVacancyAsync(vrm);
            }
            else
            {
                vm = await GetNumberOfPositionsViewModelForNewVacancyAsync(model.EmployerAccountId, ukprn);                
            }

            vm.NumberOfPositions = model.NumberOfPositions;

            return vm;
        }

        public async Task<OrchestratorResponse<Guid>> PostNumberOfPositionsEditModelAsync(VacancyRouteModel vrm, NumberOfPositionsEditModel model, VacancyUser user, long ukprn)
        {
            var numberOfPositions = int.TryParse(model.NumberOfPositions, out var n) ? n : default(int?);
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_providerVacancyClient, _recruitVacancyClient, vrm, RouteNames.NumberOfPositions_Post);
            vacancy.NumberOfPositions = numberOfPositions;
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
            mappings.Add(e => e.EmployerName, vm => vm.EmployerAccountId);

            return mappings;
        }

        private async Task ValidateEmployerAccountIdAsync(long ukprn, string employerAccountId)
        {
            var providerInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(ukprn);

            if (providerInfo.Employers.Any(e => e.EmployerAccountId == employerAccountId) == false)
                throw new AuthorisationException(string.Format(ExceptionMessages.ProviderEmployerAccountIdNotFound, ukprn, employerAccountId));
        }
    }
}
