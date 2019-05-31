using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part1.NumberOfPositions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part1
{
    public class NumberOfPositionsOrchestrator : EntityValidatingOrchestrator<Vacancy, NumberOfPositionsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.NumberOfPositions;
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;

        public NumberOfPositionsOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<NumberOfPositionsOrchestrator> logger, IReviewSummaryService reviewSummaryService) 
            : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
        }

        public NumberOfPositionsViewModel GetNumberOfPositionsViewModel()
        {
            var vm = new NumberOfPositionsViewModel
            {
                PageInfo = new PartOnePageInfoViewModel()
            };
            return vm;
        }

        public async Task<NumberOfPositionsViewModel> GetNumberOfPositionsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.NumberOfPositions_Get);

            var vm = new NumberOfPositionsViewModel
            {
                VacancyId = vacancy.Id,
                NumberOfPositions = vacancy.NumberOfPositions?.ToString(),
                PageInfo = Utility.GetPartOnePageInfo(vacancy)
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
            NumberOfPositionsViewModel vm;

            if (m.VacancyId.HasValue)
            {
                var vrm = new VacancyRouteModel { EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value };
                vm = await GetNumberOfPositionsViewModelAsync(vrm);
            }
            else
            {
                vm = GetNumberOfPositionsViewModel();
            }
            vm.NumberOfPositions = m.NumberOfPositions;
            return vm;
        }


        public async Task<OrchestratorResponse<Guid>> PostNumberOfPositionsEditModelAsync(NumberOfPositionsEditModel m, VacancyUser user)
        {
            var numberOfPositions = int.TryParse(m.NumberOfPositions, out var n)? n : default(int?);
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, 
                new VacancyRouteModel{EmployerAccountId = m.EmployerAccountId, VacancyId = m.VacancyId.Value}, RouteNames.NumberOfPositions_Post);
            vacancy.NumberOfPositions = numberOfPositions;
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
