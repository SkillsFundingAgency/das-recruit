using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses
{
    public record GetApplicationReviewsByVacancyReferenceApiResponse
    {
        public List<ApplicationReview> ApplicationReviews { get; init; }

        public record ApplicationReview
        {
            public bool HasEverBeenEmployerInterviewing { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? DateSharedWithEmployer { get; set; }
            public DateTime? ReviewedDate { get; set; }
            public DateTime? StatusUpdatedDate { get; set; }
            public DateTime? SubmittedDate { get; set; }
            public DateTime? WithdrawnDate { get; set; }
            public Guid CandidateId { get; set; }
            public Guid Id { get; set; }
            public Guid? ApplicationId { get; set; }
            public Guid? LegacyApplicationId { get; set; }
            public int Ukprn { get; set; }
            public long AccountId { get; set; }
            public long AccountLegalEntityId { get; set; }
            public long VacancyReference { get; set; }
            public string Status { get; set; }
            public string VacancyTitle { get; set; }
            public string? AdditionalQuestion1 { get; set; }
            public string? AdditionalQuestion2 { get; set; }
            public string? CandidateFeedback { get; set; }
            public string? EmployerFeedback { get; set; }
            public string? TemporaryReviewStatus { get; set; }
        }
    }
}
