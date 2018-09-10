using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;

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
        private readonly INextVacancyReviewService _nextVacancyReviewService;

        public QaVacancyClient(
                    IQueryStoreReader queryStoreReader,
                    IReferenceDataReader referenceDataReader,
                    IVacancyReviewRepository reviewRepository, 
                    IVacancyRepository vacancyRepository, 
                    IApprenticeshipProgrammeProvider apprenticeshipProgrammesProvider,
                    IMessaging messaging,
                    INextVacancyReviewService nextVacancyReviewService)
        {
            _queryStoreReader = queryStoreReader;
            _referenceDataReader = referenceDataReader;
            _reviewRepository = reviewRepository;
            _vacancyRepository = vacancyRepository;
            _apprenticeshipProgrammesProvider = apprenticeshipProgrammesProvider;
            _messaging = messaging;
            _nextVacancyReviewService = nextVacancyReviewService;
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

        public Task ApproveVacancyReviewAsync(Guid reviewId, string manualQaComment, List<ManualQaFieldIndicator> manualQaFieldIndicators)
        {
            return _messaging.SendCommandAsync(new ApproveVacancyReviewCommand
            {
                ReviewId = reviewId,
                ManualQaComment = manualQaComment,
                ManualQaFieldIndicators = manualQaFieldIndicators
            });
        }

        public Task ReferVacancyReviewAsync(Guid reviewId, string manualQaComment, List<ManualQaFieldIndicator> manualQaFieldIndicators)
        {
            return _messaging.SendCommandAsync(new ReferVacancyReviewCommand
            {
                ReviewId = reviewId,
                ManualQaComment = manualQaComment,
                ManualQaFieldIndicators = manualQaFieldIndicators
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
            var dashboard = await _queryStoreReader.GetQaDashboardAsync().ConfigureAwait(true);

            //todo - will be deleted
            var allReviews = await _reviewRepository.GetActiveAsync();
            dashboard.AllReviews = allReviews.ToList();

            return dashboard;
        }

        public async Task<List<QaVacancySummary>> GetSearchResultsAsync(string searchTerm)
        {
            if (TryGetVacancyReference(searchTerm, out var vacancyReference))
            {
                return await _reviewRepository.SearchAsync(vacancyReference);
            }
            return new List<QaVacancySummary>();
        }

        private static bool TryGetVacancyReference(string value, out long vacancyReference)
        {
            vacancyReference = 0;
            if (string.IsNullOrEmpty(value)) return false;
            
            var regex = new Regex(@"^(VAC)?(\d{10})$", RegexOptions.IgnoreCase);
            var result = regex.Match(value);
            if (result.Success)
            {
                vacancyReference = long.Parse(result.Groups[2].Value);
            }
            return result.Success;
        }

        public Task<List<QaVacancySummary>> GetVacancyReviewsInProgressAsync()
        {
            return _reviewRepository.GetVacancyReviewsInProgressAsync(_nextVacancyReviewService.GetExpiredAssignationDateTime());
        }

        public Task<Vacancy> GetVacancyAsync(long vacancyReference)
        {
            return _vacancyRepository.GetVacancyAsync(vacancyReference);
        }

        public Task<VacancyReview> GetVacancyReviewAsync(Guid reviewId)
        {
            return _reviewRepository.GetAsync(reviewId);
        }

        public Task AssignNextVacancyReviewAsync(VacancyUser user)
        {
            return _messaging.SendCommandAsync(new AssignVacancyReviewCommand
            {
                User = user
            });
        }

        public Task AssignVacancyReviewAsync(VacancyUser user, Guid reviewId)
        {
            return _messaging.SendCommandAsync(new AssignVacancyReviewCommand
            {
                User = user,
                ReviewId = reviewId
            });
        }

        public Task<int> GetApprovedCountAsync(string submittedByUserId)
        {
            return _reviewRepository.GetApprovedCountAsync(submittedByUserId);
        }

        public Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId)
        {
            return _reviewRepository.GetApprovedFirstTimeCountAsync(submittedByUserId);
        }
        
        public Task<List<VacancyReview>> GetAssignedVacancyReviewsForUserAsync(string userId)
        {            
            return _reviewRepository.GetAssignedForUserAsync(userId, _nextVacancyReviewService.GetExpiredAssignationDateTime());
        }

        public bool VacancyReviewCanBeAssigned(VacancyReview review)
        {
            return VacancyReviewCanBeAssigned(review.Status, review.ReviewedDate);
        }

        public bool VacancyReviewCanBeAssigned(ReviewStatus status, DateTime? reviewedDate)
        {
            return _nextVacancyReviewService.VacancyReviewCanBeAssigned(status, reviewedDate);
        }

    }
}