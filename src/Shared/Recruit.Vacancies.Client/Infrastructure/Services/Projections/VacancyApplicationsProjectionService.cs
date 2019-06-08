using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class VacancyApplicationsProjectionService : IVacancyApplicationsProjectionService
    {
        private readonly ILogger<VacancyApplicationsProjectionService> _logger;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApplicationReviewQuery _applicationReviewQuery;
        private readonly IQueryStoreWriter _writer;

        public VacancyApplicationsProjectionService(ILogger<VacancyApplicationsProjectionService> logger, IVacancyRepository vacancyRepository, IApplicationReviewQuery applicationReviewQuery, IQueryStoreWriter writer)
        {
            _logger = logger;
            _vacancyRepository = vacancyRepository;
            _applicationReviewQuery = applicationReviewQuery;
            _writer = writer;
        }

        public async Task UpdateVacancyApplicationsAsync(long vacancyReference)
        {
            _logger.LogInformation("Updating vacancyApplications projection for vacancyReference: {vacancyReference}", vacancyReference);

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);
            var vacancyApplicationReviews = await _applicationReviewQuery.GetForVacancyAsync<ApplicationReview>(vacancy.VacancyReference.Value);

            var vacancyApplications = new VacancyApplications
            {
                VacancyReference = vacancy.VacancyReference.Value,
                Applications = vacancyApplicationReviews.Select(MapToVacancyApplication).ToList()
            };

            await _writer.UpdateVacancyApplicationsAsync(vacancyApplications);
        }

        private VacancyApplication MapToVacancyApplication(ApplicationReview review)
        {
            var projection = new VacancyApplication
            {
                Status = review.Status,
                SubmittedDate = review.SubmittedDate,
                ApplicationReviewId = review.Id,
                IsWithdrawn = review.IsWithdrawn,
                FirstName = string.Empty,
                LastName = string.Empty,
                DateOfBirth = review.Application.BirthDate,
                DisabilityStatus = ApplicationReviewDisabilityStatus.Unknown
            };

            if (review.IsWithdrawn == false)
            {
                projection.FirstName = review.Application.FirstName;
                projection.LastName = review.Application.LastName;
                projection.DisabilityStatus = review.Application.DisabilityStatus ?? ApplicationReviewDisabilityStatus.Unknown;
            }

            return projection;
        }
    }
}
