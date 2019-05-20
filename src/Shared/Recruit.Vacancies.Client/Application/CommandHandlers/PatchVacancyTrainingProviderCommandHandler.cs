using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class PatchVacancyTrainingProviderCommandHandler : IRequestHandler<PatchVacancyTrainingProviderCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ILogger<PatchVacancyTrainingProviderCommandHandler> _logger;
        private readonly ITrainingProviderService _trainingProviderService;

        private readonly TrainingProvider _esfaTrainingProvider = new TrainingProvider
        {
            Ukprn = 10033670,
            Name = "Education Skills Funding Agency",
            Address = new Address
            {
                AddressLine1 = "Cheylesmore House",
                AddressLine2 = "Quinton Road",
                AddressLine3 = "Coventry",
                AddressLine4 = "",
                Postcode = "CV1 2WT"
            }
        };

        public PatchVacancyTrainingProviderCommandHandler(
            ILogger<PatchVacancyTrainingProviderCommandHandler> logger,
            IVacancyRepository repository,
            IMessaging messaging,
            ITrainingProviderService trainingProviderService)
        {
            _repository = repository;
            _messaging = messaging;
            _logger = logger;
            _trainingProviderService = trainingProviderService;
        }

        public async Task Handle(PatchVacancyTrainingProviderCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (vacancy.OwnerType == OwnerType.Employer)
            {
                _logger.LogInformation("Vacancy: {vacancyId} already is employer owned so will not backfill provider info.", vacancy.Id);
                return;
            }

            if (!string.IsNullOrEmpty(vacancy.TrainingProvider.Name))
            {
                _logger.LogInformation("Vacancy: {vacancyId} already has training provider info backfilled. Will not be changed.", vacancy.Id);
                return;
            }

            _logger.LogInformation("Patching training provider name and address for vacancy {vacancyId}.", message.VacancyId);

            TrainingProvider tp;

            if (vacancy.TrainingProvider.Ukprn.Value.Equals(_esfaTrainingProvider.Ukprn))
            {
                tp = _esfaTrainingProvider;
            }
            else
            {
                tp = await _trainingProviderService.GetProviderAsync(vacancy.TrainingProvider.Ukprn.Value);
            }

            vacancy = await _repository.GetVacancyAsync(message.VacancyId);
            PatchVacancyTrainingProvider(vacancy, tp);

            await _repository.UpdateAsync(vacancy);

            _logger.LogInformation("Updated Vacancy: {vacancyId} with training provider name and address", vacancy.Id);
        }

        private void PatchVacancyTrainingProvider(Esfa.Recruit.Vacancies.Client.Domain.Entities.Vacancy vacancy, Esfa.Recruit.Vacancies.Client.Domain.Entities.TrainingProvider tp)
        {
            vacancy.TrainingProvider.Name = tp.Name;
            vacancy.TrainingProvider.Address = tp.Address;
        }
    }
}