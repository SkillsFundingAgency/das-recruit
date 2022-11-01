using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Submitted
{
    public class VacancySubmittedConfirmationViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }
        public bool IsResubmit { get; set; }
        public bool HasNotificationsSet { get; set; }

        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
    }
}
