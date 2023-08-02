using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewFeedbackViewModel : ApplicationReviewStatusChangeModel
    {
        public string Name { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(Outcome)
        };
    }
}