using System.Threading.Tasks;
using Esfa.Recruit.QA.Web.ViewModels.ManageProvider;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.QA.Web.Orchestrators
{
    public class UnblockOrganisationOrchestrator
    {
        private readonly IBlockedOrganisationQuery _blockedOrganisationQuery;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly ITrainingProviderService _trainingProviderService;
        public UnblockOrganisationOrchestrator(
            IBlockedOrganisationQuery blockedOrganisationQuery,
            IMessaging messaging, ITimeProvider timeProvider, ITrainingProviderService trainingProviderService)
        {
            _blockedOrganisationQuery = blockedOrganisationQuery;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _trainingProviderService = trainingProviderService;
        }

        public async Task<ProviderUnblockedAcknowledgementViewModel> GetAcknowledgementViewModelAsync(long ukprn)
        {
            var providerDetail = await _trainingProviderService.GetProviderAsync(ukprn);

            return new ProviderUnblockedAcknowledgementViewModel
            {
                Name = providerDetail.Name,
                Ukprn = ukprn
            };
        }

        public async Task<bool> IsProviderAlreadyBlocked(long ukprn)
        {
            var blockedOrganisation = await _blockedOrganisationQuery.GetByOrganisationIdAsync(ukprn.ToString());
            return blockedOrganisation?.BlockedStatus == BlockedStatus.Blocked;
        }

        public Task UnblockProviderAsync(long ukprn, VacancyUser user)
        {
            var command = new UnblockProviderCommand(ukprn, user, _timeProvider.Now);
            return _messaging.SendCommandAsync(command);
        }

        public async Task<ConfirmTrainingProviderUnblockingEditModel> GetConfirmTrainingProviderUnblockingViewModel(long ukprn)
        {
            var provider = await _trainingProviderService.GetProviderAsync(ukprn);
            return ConvertToConfirmViewModel(provider);
        }

        private ConfirmTrainingProviderUnblockingEditModel ConvertToConfirmViewModel(TrainingProvider provider)
        {
            return new ConfirmTrainingProviderUnblockingEditModel
            {
                Ukprn = provider.Ukprn.GetValueOrDefault(),
                ProviderName = provider.Name
            };
        }
    }
}