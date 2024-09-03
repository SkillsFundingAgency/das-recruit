using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;

public class ApplicationStatusConfirmationViewModel : ApplicationReviewRouteModel
{
    public string Name { get; set; }              
    public ApplicationReviewStatus? Outcome { get; set; }
    public string CandidateFeedback { get; set; }   
    public bool? NotifyCandidate { get; set; }
    public string FriendlyId { get; set; }
    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(Outcome)
    };

    public bool ShowStatusSuccessful => Outcome.Value == ApplicationReviewStatus.Successful;
    public bool ShowStatusUnsuccessful => Outcome.Value == ApplicationReviewStatus.Unsuccessful;
    public string YesRadioButtonText => ShowStatusSuccessful ? "Yes, make this application successful and notify the applicant" : "Yes, notify the applicant";
    public string NoRadioButtonText => ShowStatusSuccessful ? "No, do not make this application successful" : "No";
}