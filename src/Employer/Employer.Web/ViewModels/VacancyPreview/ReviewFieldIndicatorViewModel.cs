using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class ReviewFieldIndicatorViewModel
    {
        public ReviewFieldIndicatorViewModel(string reviewFieldIdentifier, string anchor, string text)
        {
            ReviewFieldIdentifier = reviewFieldIdentifier;
            Anchor = anchor;
            Texts = new List<string> { text };
        }

        public string ReviewFieldIdentifier { get; set; }
        public string Anchor { get; set; }
        public IList<string> Texts { get; set; }
    }
}
