using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class PatchVacancyTrainingProviderCommandHandler : IRequestHandler<PatchVacancyTrainingProviderCommand, Unit>
    {
        private readonly IVacancyRepository _repository;
        private readonly ILogger<PatchVacancyTrainingProviderCommandHandler> _logger;
        private readonly ITrainingProviderService _trainingProviderService;

        public PatchVacancyTrainingProviderCommandHandler(
            ILogger<PatchVacancyTrainingProviderCommandHandler> logger,
            IVacancyRepository repository,
            ITrainingProviderService trainingProviderService)
        {
            _repository = repository;
            _logger = logger;
            _trainingProviderService = trainingProviderService;
        }

        public async Task<Unit> Handle(PatchVacancyTrainingProviderCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);

            if (vacancy.OwnerType == OwnerType.Employer)
            {
                _logger.LogInformation("Vacancy: {vacancyId} already is employer owned so will not backfill provider info.", vacancy.Id);
                return Unit.Value;
            }

            if (!string.IsNullOrEmpty(vacancy.TrainingProvider.Name))
            {
                _logger.LogInformation("Vacancy: {vacancyId} already has training provider info backfilled. Will not be changed.", vacancy.Id);
                return Unit.Value;
            }

            _logger.LogInformation("Patching training provider name and address for vacancy {vacancyId}.", message.VacancyId);

            TrainingProvider tp;

            if (vacancy.TrainingProvider.Ukprn.Value.Equals(EsfaTestTrainingProvider.Ukprn))
            {
                tp = GetEsfaTestTrainingProvider();
            }
            else
            {
                tp = await _trainingProviderService.GetProviderAsync(vacancy.TrainingProvider.Ukprn.Value);
            }

            vacancy = await _repository.GetVacancyAsync(message.VacancyId);
            PatchVacancyTrainingProvider(vacancy, tp);

            await _repository.UpdateAsync(vacancy);

            _logger.LogInformation("Updated Vacancy: {vacancyId} with training provider name and address", vacancy.Id);
            
            return Unit.Value;
        }

        private void PatchVacancyTrainingProvider(Esfa.Recruit.Vacancies.Client.Domain.Entities.Vacancy vacancy, Esfa.Recruit.Vacancies.Client.Domain.Entities.TrainingProvider tp)
        {
            if (tp == null) 
            {
                _logger.LogError($"Unable to patch training provider for vacancy {vacancy.VacancyReference}");
                return;
            }
            vacancy.TrainingProvider.Name = tp.Name;
            vacancy.TrainingProvider.Address = tp.Address;
        }

        private TrainingProvider GetEsfaTestTrainingProvider()
        {
            return new TrainingProvider
            {
                Ukprn = EsfaTestTrainingProvider.Ukprn,
                Name = EsfaTestTrainingProvider.Name,
                Address = new Address
                {
                    AddressLine1 = EsfaTestTrainingProvider.AddressLine1,
                    AddressLine2 = EsfaTestTrainingProvider.AddressLine2,
                    AddressLine3 = EsfaTestTrainingProvider.AddressLine3,
                    AddressLine4 = EsfaTestTrainingProvider.AddressLine4,
                    Postcode = EsfaTestTrainingProvider.Postcode
                }
            };
        }
    }
}