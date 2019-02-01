using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class ConsiderationsOrchestrator : EntityValidatingOrchestrator<Vacancy, ConsiderationsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.ThingsToConsider;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IMessaging _messaging;

        public ConsiderationsOrchestrator(
            ILogger<ConsiderationsOrchestrator> logger, 
            IProviderVacancyClient client, 
            IRecruitVacancyClient vacancyClient, 
            IReviewSummaryService reviewSummaryService,
            IMessaging messaging) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _messaging = messaging;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Considerations_Get);
            
            var vm = new ConsiderationsViewModel
            {
                Title = vacancy.Title,
                ThingsToConsider = vacancy.ThingsToConsider,
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                //vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                //    ReviewFieldMappingLookups.GetConsiderationsFieldIndicators());
            }

            return vm;
        }

        public async Task<ConsiderationsViewModel> GetConsiderationsViewModelAsync(ConsiderationsEditModel m)
        {
            var vm = await GetConsiderationsViewModelAsync((VacancyRouteModel)m);

            vm.ThingsToConsider = m.ThingsToConsider;

            return vm;
        }

        public async Task<OrchestratorResponse> PostConsiderationsEditModelAsync(ConsiderationsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.Considerations_Post);
            
            vacancy.ThingsToConsider = m.ThingsToConsider;

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _messaging.SendCommandAsync(new UpdateDraftVacancyCommand
                    {
                        Vacancy = vacancy,
                        User = user
                    })
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, ConsiderationsEditModel>();

            mappings.Add(e => e.ThingsToConsider, vm => vm.ThingsToConsider);

            return mappings;
        }
    }
}
