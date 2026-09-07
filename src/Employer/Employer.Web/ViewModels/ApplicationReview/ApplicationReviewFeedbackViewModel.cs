using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;

public class ApplicationReviewFeedbackViewModel : ApplicationReviewStatusChangeModel
{
    public string Name { get; set; }
    public string FriendlyId { get; set; }
    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(Outcome)
    };
}
