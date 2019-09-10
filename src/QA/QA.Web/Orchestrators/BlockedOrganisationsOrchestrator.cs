using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.QA.Web.ViewModels.ManageProvider;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.QA.Web.Orchestrators
{
    public class BlockedOrganisationsOrchestrator
    {
        private readonly IBlockedOrganisationQuery _blockedOrganisationQuery;
        private readonly IQueryStoreReader _queryStore;
        private readonly IMessaging _messaging;
        private readonly ITimeProvider _timeProvider;
        private readonly ITrainingProviderService _trainingProviderService;
        public BlockedOrganisationsOrchestrator(
            IBlockedOrganisationQuery blockedOrganisationQuery, IQueryStoreReader queryStore,
            IMessaging messaging, ITimeProvider timeProvider, ITrainingProviderService trainingProviderService)
        {
            _blockedOrganisationQuery = blockedOrganisationQuery;
            _queryStore = queryStore;
            _messaging = messaging;
            _timeProvider = timeProvider;
            _trainingProviderService = trainingProviderService;
        }

        public async Task<ConfirmTrainingProviderBlockingViewModel> GetConfirmTrainingProviderBlockingViewModelAsync(long ukprn)
        {
            var providerDetailTask = _trainingProviderService.GetProviderAsync(ukprn);
            var providerEditVacancyInfoTask = _queryStore.GetProviderVacancyDataAsync(ukprn);
            await Task.WhenAll(providerEditVacancyInfoTask, providerDetailTask);
            var providerDetail = providerDetailTask.Result;             
            var providerVacancyInfo = providerEditVacancyInfoTask.Result;

            var permissionCount = 0;
            if(providerVacancyInfo?.Employers != null)
            {
                permissionCount = providerVacancyInfo.Employers.SelectMany(e => e.LegalEntities).Count();
            }
            
            return ConvertToConfirmViewModel(providerDetail, permissionCount);
        }
        public async Task<ConsentForProviderBlockingViewModel> GetConsentForProviderBlockingViewModelAsync(long ukprn)
        {
            var providerDetailTask = _trainingProviderService.GetProviderAsync(ukprn);
            var providerEditVacancyInfoTask = _queryStore.GetProviderVacancyDataAsync(ukprn);
            await Task.WhenAll(providerEditVacancyInfoTask, providerDetailTask);
            var providerDetail = providerDetailTask.Result;             
            var providerVacancyInfo = providerEditVacancyInfoTask.Result;

            var permissionCount = 0;
            if(providerVacancyInfo?.Employers != null)
            {
                permissionCount = providerVacancyInfo.Employers.SelectMany(e => e.LegalEntities).Count();
            }
            
            return ConvertToConsentViewModel(providerDetail, permissionCount);
        }

        public Task BlockProviderAsync(long ukprn, string reason, VacancyUser user)
        {
            var command = new BlockProviderCommand(ukprn, user, _timeProvider.Now, reason);
            return _messaging.SendCommandAsync(command);
        }

        public async Task<ProviderBlockedAcknowledgementViewModel> GetAcknowledgementViewModelAsync(long ukprn)
        {
            var providerDetail = await _trainingProviderService.GetProviderAsync(ukprn);

            return new ProviderBlockedAcknowledgementViewModel 
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

        public async Task<ProviderAlreadyBlockedViewModel> GetProviderAlreadyBlockedViewModelAsync(long ukprn)
        {
            var provider = await _trainingProviderService.GetProviderAsync(ukprn);
            return new ProviderAlreadyBlockedViewModel{ Ukprn = ukprn, Name = provider.Name };
        }

        public async Task<BlockedOrganisationsViewModel> GetBlockedOrganisationsViewModel()
        {
            var blockedProviders = await _queryStore.GetBlockedProvidersAsync();

            if (blockedProviders?.Data == null) return new BlockedOrganisationsViewModel();

            var blockedOrganisationViewModels = new List<BlockedOrganisationViewModel>();

            foreach(var provider in blockedProviders.Data)
            {
                blockedOrganisationViewModels.Add(ConvertToBlockedOrganisationViewModel(provider));
            }

            foreach( var vm in blockedOrganisationViewModels)
            {
                var ukprn = long.Parse(vm.OrganisationId);
                var prov = await _trainingProviderService.GetProviderAsync(ukprn);
                vm.OrganisationName = prov?.Name;
                vm.Postcode = prov?.Address?.Postcode;
            }

            return new BlockedOrganisationsViewModel { BlockedOrganisations = blockedOrganisationViewModels };
        }

        private BlockedOrganisationViewModel ConvertToBlockedOrganisationViewModel(BlockedOrganisationSummary summary) 
        {
            return new BlockedOrganisationViewModel
            {
                OrganisationId = summary.BlockedOrganisationId,
                BlockedOn = summary.BlockedDate.ToUkTime().AsGdsDateTime(),
                BlockedBy = summary.BlockedByUser
            };
        }

        private ConfirmTrainingProviderBlockingViewModel ConvertToConfirmViewModel(TrainingProvider provider, int permissionCount)
        {
            return new ConfirmTrainingProviderBlockingViewModel
            {
                Ukprn = provider.Ukprn.GetValueOrDefault(),
                Name = provider.Name,
                Address = provider.Address.ToAddressString(),
                PermissionCount = permissionCount
            };
        }
        private ConsentForProviderBlockingViewModel ConvertToConsentViewModel(TrainingProvider provider, int permissionCount)
        {
            return new ConsentForProviderBlockingViewModel
            {
                Name = provider.Name,
                PermissionCount = permissionCount
            };
        }
    }
}