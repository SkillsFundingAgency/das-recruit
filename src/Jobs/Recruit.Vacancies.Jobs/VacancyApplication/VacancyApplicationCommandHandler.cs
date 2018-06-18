using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyApplication
{
    public class VacancyApplicationCommandHandler
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly ILogger<VacancyApplicationCommandHandler> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly IApplicationReviewRepository _applicationReviewRepository;

        public VacancyApplicationCommandHandler(IVacancyRepository vacancyRepository, IApplicationReviewRepository applicationReviewRepository, ILogger<VacancyApplicationCommandHandler> logger, ITimeProvider timeProvider)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _applicationReviewRepository = applicationReviewRepository;
            _timeProvider = timeProvider;
        }

        public async Task Handle(ApplicationSubmitCommand command)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(command.Application.VacancyReference);

            var review = new ApplicationReview
            {
                Id = Guid.NewGuid(),
                VacancyReference = vacancy.VacancyReference.Value,
                Application = command.Application,
                CandidateId = command.Application.CandidateId,
                CreatedDate = _timeProvider.Now,
                EmployerAccountId = vacancy.EmployerAccountId,
                Status = ApplicationReviewStatus.New,
                SubmittedDate = command.Application.ApplicationDate
            };

            await _applicationReviewRepository.CreateAsync(review);
        }
    }
}
