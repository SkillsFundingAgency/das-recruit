using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications
{
    public class ManageNotificationsAcknowledgementViewModel
    {
        [FromRoute]
        public long Ukprn { get; set; }
        public string UserEmail { get; set; }
        public bool IsVacancyRejectedSelected { get; set; }
        public bool IsVacancyRejectedByEmployerSelected { get; set; }
        public bool IsVacancyClosingSoonSelected { get; set; }
        public bool IsApplicationSubmittedSelected { get; set; }
        public string Frequency { get; set; }
        public bool IsUserSubmittedVacanciesSelected { get; set; }
    }
}