namespace Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications
{
    public class ManageNotificationsAcknowledgementViewModel
    {
        public string UserEmail { get; set; }
        public bool IsVacancyRejectedSelected { get; set; }
        public bool IsVacancyClosingSoonSelected { get; set; }
        public bool IsApplicationSubmittedSelected { get; set; }
        public string Frequency { get; set; }
        public string IsUserSubmittedVacanciesSelected { get; set; }
    }
}