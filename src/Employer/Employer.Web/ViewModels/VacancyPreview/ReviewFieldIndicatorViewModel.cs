namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class ReviewFieldIndicatorViewModel
    {
        public ReviewFieldIndicatorViewModel(string reviewFieldIdentifier, string anchor, string text)
        {
            ReviewFieldIdentifier = reviewFieldIdentifier;
            Anchor = anchor;
            Text = text;
        }

        public string ReviewFieldIdentifier { get; set; }
        public string Anchor { get; set; }
        public string Text { get; set; }
    }
}
