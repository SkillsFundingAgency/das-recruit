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

        public bool ShowStatusSuccessful => Outcome.Value == ApplicationReviewStatus.Successful;
        public bool ShowStatusUnSuccessful => Outcome.Value == ApplicationReviewStatus.Unsuccessful &&
                                              Status != ApplicationReviewStatus.EmployerUnsuccessful;
        public bool ShowStatusEmployerUnSuccessful => Outcome.Value == ApplicationReviewStatus.Unsuccessful &&
                                                      Status == ApplicationReviewStatus.EmployerUnsuccessful;
        public ApplicationReviewStatus? Status { get; set; }
        public string YesMessageText => ShowStatusSuccessful
            ?
            "Yes, make this application successful and notify the applicant"
            :
            ShowStatusUnSuccessful
                ? "Yes, notify the applicant"
                :
                "Yes";
        public string NoMessageText => ShowStatusSuccessful ? "No, do not make this application successful" : "No";
        public long Ukprn { get; set; }
        public Guid? VacancyId { get; set; }
        public Dictionary<string, string> RouteDictionary
        {
            get
            {
                var routeDictionary = new Dictionary<string, string>
                {
                    {"Ukprn", Ukprn.ToString()}
                };
                if (VacancyId != null)
                {
                    routeDictionary.Add("VacancyId", VacancyId.ToString());
                }
                return routeDictionary;
            }
        }
    }
}
