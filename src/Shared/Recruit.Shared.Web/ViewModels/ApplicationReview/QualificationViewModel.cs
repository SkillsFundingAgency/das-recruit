namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview
{
    public class QualificationViewModel
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public bool IsPredicted { get; set; }
        public int Year { get; set; }
        public string AdditionalInformation { get; set; }
    }
}