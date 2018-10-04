using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class ReviewFieldIndicatorViewModel
    {
        public ReviewFieldIndicatorViewModel(string reviewFieldIdentifier, string anchor)
        {
            ReviewFieldIdentifier = reviewFieldIdentifier;
            Anchor = anchor;
            AutoQaTexts = new List<string>();
        }

        public string ReviewFieldIdentifier { get; }
        public string Anchor { get; }
        public string ManualQaText { get; set; }
        public List<string> AutoQaTexts { get; }
    }
}
