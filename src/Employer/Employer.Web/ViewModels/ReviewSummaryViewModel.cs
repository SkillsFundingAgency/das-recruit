using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ReviewSummaryViewModel
    {
        public bool CanDisplayReviewHeader { get; internal set; }
        public string ReviewerComments { get; internal set; }
        public IEnumerable<ReviewFieldIndicatorViewModel> FieldIndicators { get; internal set; } = new List<ReviewFieldIndicatorViewModel>();
    }
}
