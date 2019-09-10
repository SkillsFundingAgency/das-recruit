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
    public class UnBlockOrganisationOrchestrator
    {
        private readonly IBlockedOrganisationQuery _blockedOrganisationQuery;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly ITrainingProviderService _trainingProviderService;
        public UnBlockOrganisationOrchestrator(
            IBlockedOrganisationQuery blockedOrganisationQuery,
            IMessaging messaging, ITimeProvider timeProvider, ITrainingProviderService trainingProviderService)
        {
            _blockedOrganisationQuery = blockedOrganisationQuery;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _trainingProviderService = trainingProviderService;
        }

        public async Task<ProviderUnBlockedAcknowledgementViewModel> GetAcknowledgementViewModelAsync(long ukprn)
        {
            var providerDetail = await _trainingProviderService.GetProviderAsync(ukprn);

            return new ProviderUnBlockedAcknowledgementViewModel
            {
                Name = providerDetail.Name,
                Ukprn = ukprn
            };
        }

        public async Task<bool> IsProviderAlreadyBlocked(long ukprn)
        {
            var blockedOrganisation = await _blockedOrganisationQuery.GetByOrganisationIdAsync(ukprn.ToString());
            return blockedOrganisation != null;
        }

        public Task UnBlockProviderAsync(long ukprn, VacancyUser user)
        {
            var command = new UnBlockProviderCommand(ukprn, user, _timeProvider.Now);
            return _messaging.SendCommandAsync(command);
        }

        public async Task<UnBlockTrainingProviderEditModel> GetBlockedOrganisationViewModel(long ukprn)
        {
            var provider = await _trainingProviderService.GetProviderAsync(ukprn);
            return new UnBlockTrainingProviderEditModel { Ukprn = ukprn, ProviderName = provider.Name };
        }
    }
}