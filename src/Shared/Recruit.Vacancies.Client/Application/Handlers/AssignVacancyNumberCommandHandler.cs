﻿using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Handlers
{
    public class AssignVacancyNumberCommandHandler: IRequestHandler<AssignVacancyNumberCommand>
    {
        private readonly IVacancyRepository _repository;
        private readonly IMessaging _messaging;
        private readonly ILogger<AssignVacancyNumberCommandHandler> _logger;
        private readonly IGenerateVacancyNumbers _generator;

        public AssignVacancyNumberCommandHandler(
            IVacancyRepository repository, 
            IMessaging messaging, 
            ILogger<AssignVacancyNumberCommandHandler> logger, 
            IGenerateVacancyNumbers generator)
        {
            _repository = repository;
            _messaging = messaging;
            _logger = logger;
            _generator = generator;
        }

        public async Task Handle(AssignVacancyNumberCommand message, CancellationToken cancellationToken)
        {
            var vacancy = await _repository.GetVacancyAsync(message.VacancyId);
            
            if (vacancy.VacancyNumber.HasValue)
            {
                _logger.LogWarning("Vacancy: {vacancyId} already has a vacancy number: {vacancyNumber}. Will not be changed.", vacancy.Id, vacancy.VacancyNumber);
            }

            vacancy.VacancyNumber = await _generator.GenerateAsync();

            await _repository.UpdateAsync(vacancy);

            _logger.LogInformation("Updated Vacancy: {vacancyId} with vacancy number: {vacancyNumber}", vacancy.Id, vacancy.VacancyNumber);
        }
    }
}
