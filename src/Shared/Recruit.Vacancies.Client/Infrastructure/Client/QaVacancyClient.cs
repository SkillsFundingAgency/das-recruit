using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public class QaVacancyClient : IQaVacancyClient
    {
        private readonly IQueryStoreReader _queryStoreReader;
        private readonly IReferenceDataReader _referenceDataReader;
        private readonly IVacancyReviewRepository _reviewRepository;
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IApprenticeshipProgrammeProvider _apprenticeshipProgrammesProvider;
        private readonly IMessaging _messaging;

        public QaVacancyClient(
                    IQueryStoreReader queryStoreReader,
                    IReferenceDataReader referenceDataReader,
                    IVacancyReviewRepository reviewRepository, 
                    IVacancyRepository vacancyRepository, 
                    IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
                    IMessaging messaging)
        {
            _queryStoreReader = queryStoreReader;
            _referenceDataReader = referenceDataReader;
            _reviewRepository = reviewRepository;
            _vacancyRepository = vacancyRepository;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _messaging = messaging;
        }

        public Task ApproveReferredReviewAsync(Guid reviewId, string shortDescription, string vacancyDescription, string trainingDescription, string outcomeDescription, string thingsToConsider, string employerDescription)
        {
            return _messaging.SendCommandAsync(new ApproveReferredVacancyReviewCommand
            {
                ReviewId = reviewId,
                ShortDescription = shortDescription,
                VacancyDescription = vacancyDescription,
                TrainingDescription = trainingDescription,
                OutcomeDescription = outcomeDescription,
                ThingsToConsider = thingsToConsider,
                EmployerDescription = employerDescription
            });
        }

        public Task ApproveReview(Guid reviewId)
        {
            return _messaging.SendCommandAsync(new ApproveVacancyReviewCommand
            {
                ReviewId = reviewId
            });
        }

        public Task<IApprenticeshipProgramme> GetApprenticeshipProgrammeAsync(string programmeId)
        {
            return _apprenticeshipProgrammesProvider.GetApprenticeshipProgrammeAsync(programmeId);
        }

        public Task<Qualifications> GetCandidateQualificationsAsync()
        {
            return _referenceDataReader.GetReferenceData<Qualifications>();
        }

        public async Task<QaDashboard> GetDashboardAsync()
        {
            var dashboard = await _queryStoreReader.GetQaDashboardAsync();

            //todo - will be deleted
            var allReviews = await _reviewRepository.GetActiveAsync();
            dashboard.AllReviews = allReviews.ToList();
            return dashboard;
        }

        public Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            return _vacancyRepository.GetVacancyAsync(vacancyReference);
        }

        public Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId)
        {
            return _reviewRepository.GetAsync(reviewId);
        }

        public Task ReferVacancyReviewAsync(Guid reviewId)
        {
            return _messaging.SendCommandAsync(new ReferVacancyReviewCommand
            {
                ReviewId = reviewId
            });
        }

        public Task StartReview(Guid reviewId, VacancyUser user)
        {
            return _messaging.SendCommandAsync(new StartVacancyReviewCommand
            {
                ReviewId = reviewId,
                User = user
            });
        }
    }
}