using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.DomainEvents.Handlers.Provider
{
    public class SetupProviderHandler : DomainEventHandler, IDomainEventHandler<SetupProviderEvent>
    {
        private readonly ILogger<SetupProviderHandler> _logger;
        private readonly IEditVacancyInfoProjectionService _projectionService;
        private readonly IProviderRelationshipsService _providerRelationshipService;
        private readonly IEmployerVacancyClient _client;
        private readonly IPasAccountProvider _pasAccountProvider;

        public SetupProviderHandler(ILogger<SetupProviderHandler> logger,
            IEditVacancyInfoProjectionService projectionService,
            IProviderRelationshipsService providerRelationshipService,
            IEmployerVacancyClient client,
            IPasAccountProvider pasAccountProvider) : base(logger)
        {
            _logger = logger;
            _projectionService = projectionService;
            _providerRelationshipService = providerRelationshipService;
            _client = client;
            _pasAccountProvider = pasAccountProvider;
        }

        public async Task HandleAsync(string eventPayload)
        {
            var eventData = DeserializeEvent<SetupProviderEvent>(eventPayload);

            try
            {
                _logger.LogInformation($"Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}}", eventData.Ukprn);

                var employerInfosTask = _providerRelationshipService.GetLegalEntitiesForProviderAsync(eventData.Ukprn, OperationType.Recruitment, OperationType.RecruitmentRequiresReview);
                var providerAgreementTask = _pasAccountProvider.HasAgreementAsync(eventData.Ukprn);

                await Task.WhenAll(employerInfosTask, providerAgreementTask);

                var employerInfos = employerInfosTask.Result;

                foreach (var employerInfo in employerInfos)
                {
                    await _client.SetupEmployerAsync(employerInfo.EmployerAccountId);
                }

                await _projectionService.UpdateProviderVacancyDataAsync(eventData.Ukprn, employerInfos, providerAgreementTask.Result);

                _logger.LogInformation($"Finished Processing {nameof(SetupProviderEvent)} for Ukprn: {{Ukprn}} has agreement:{providerAgreementTask.Result}", eventData.Ukprn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process {eventBody}", eventData);
                throw;
            }
        }
    }
}