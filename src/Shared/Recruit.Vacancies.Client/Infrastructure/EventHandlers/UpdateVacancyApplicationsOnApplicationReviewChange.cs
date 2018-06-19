using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.EventHandlers
{
    public class UpdateVacancyApplicationsOnApplicationReviewChange : INotificationHandler<ApplicationReviewCreatedEvent>
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApplicationReviewRepository _applicationReviewRepository;
        private readonly IQueryStoreWriter _writer;
        
        public UpdateVacancyApplicationsOnApplicationReviewChange(IVacancyRepository vacancyRepository, IApplicationReviewRepository applicationReviewRepository, IQueryStoreWriter writer)
        {
            _vacancyRepository = vacancyRepository;
            _applicationReviewRepository = applicationReviewRepository;
            _writer = writer;
        }

        public Task Handle(ApplicationReviewCreatedEvent notification, CancellationToken cancellationToken)
        {
            return Handle(notification.VacancyId);
        }

        public async Task Handle(Guid vacancyId)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);
            var vacancyApplicationReviews = await _applicationReviewRepository.GetApplicationReviewsForVacancyAsync<ApplicationReview>(vacancy.VacancyReference.Value);

            var vacancyApplications = new VacancyApplications
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Applications = vacancyApplicationReviews.Select(r => new VacancyApplication
                {
                    Status = r.Status,
                    SubmittedDate = r.SubmittedDate,
                    ApplicationReviewId = r.Id,
                    CandidateName = r.Application.FullName
                }).ToList()
            };

            await _writer.UpdateVacancyApplicationsAsync(vacancyApplications);
        }

    }
}
