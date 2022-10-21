using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications
{
    public class ManageNotificationsAcknowledgementViewModel : ManageNotificationsRouteModel
    {
        public string UserEmail { get; set; }
        public bool IsVacancyRejectedSelected { get; set; }
        public bool IsVacancySentForEmployerReviewSelected { get; set; }
        public bool IsVacancyClosingSoonSelected { get; set; }
        public bool IsApplicationSubmittedSelected { get; set; }
        public string Frequency { get; set; }
        public bool IsUserSubmittedVacanciesSelected { get; set; }
    }
}