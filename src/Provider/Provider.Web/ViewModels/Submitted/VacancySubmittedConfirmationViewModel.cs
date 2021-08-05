namespace Esfa.Recruit.Provider.Web.ViewModels.Submitted
{
    public class VacancySubmittedConfirmationViewModel
    {
        public string Title { get; set; }
        public string VacancyReference { get; set; }
        public bool IsResubmit { get; set; }

        public bool HasVacancyReference => !string.IsNullOrEmpty(VacancyReference);
        public bool HasNotificationsSet { get; set; }
        public bool IsVacancyRejectedByESFANotificationSelected { get; set; }
    }
}
