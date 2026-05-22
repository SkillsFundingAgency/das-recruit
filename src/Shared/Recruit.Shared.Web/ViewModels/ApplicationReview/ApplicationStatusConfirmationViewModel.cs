using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview
{
    public class ApplicationStatusConfirmationViewModel
    {
        public string Name { get; set; }
        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }
        public string FriendlyId { get; set; }
        public Guid ApplicationReviewId { get; set; }
        public bool? NotifyCandidate { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Outcome)
        };

        public bool ShowStatusSuccessful => Outcome.GetValueOrDefault() == ApplicationReviewStatus.Successful;
        public bool ShowStatusUnsuccessful => Outcome.GetValueOrDefault() == ApplicationReviewStatus.Unsuccessful &&
                                              Status != ApplicationReviewStatus.EmployerUnsuccessful;
        public bool ShowStatusEmployerUnsuccessful => Outcome.GetValueOrDefault() == ApplicationReviewStatus.Unsuccessful &&
                                                      Status == ApplicationReviewStatus.EmployerUnsuccessful;
        public ApplicationReviewStatus? Status { get; set; }
        public string YesMessageText => ShowStatusSuccessful
            ? "Yes, make this application successful and notify the applicant"
            : ShowStatusUnsuccessful
                ? "Yes, make this application unsuccessful and notify the applicant"
                : "Yes";
        public string NoMessageText => ShowStatusSuccessful
            ? "No, leave the application unchanged"
            : ShowStatusUnsuccessful
                ? "No, leave the application unchanged"
                : "No";
        public long Ukprn { get; set; }
        public Guid? VacancyId { get; set; }
    }
}