using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class JobAdvertConfirmationViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }
        public bool ApprovedJobAdvert { get; set; }
        public bool RejectedJobAdvert { get; set; }        
        public string TrainingProviderName { get; set; }
        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
        public string FindAnApprenticeshipUrl { get; set; }
    }
}
