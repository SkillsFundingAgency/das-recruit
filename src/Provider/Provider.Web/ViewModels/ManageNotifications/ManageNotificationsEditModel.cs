using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications
{
    public class ManageNotificationsEditModel
    {
        public bool IsVacancyRejectedSelected { get; set; }
        public bool IsVacancyRejectedByEmployerSelected { get; set; }
        public bool IsVacancyClosingSoonSelected { get; set; }
        public bool IsApplicationSubmittedSelected { get; set; }
        public NotificationFrequency? NotificationFrequency { get; set; }
        public NotificationScope? NotificationScope { get; set; }
        public bool HasAnySubscription => IsVacancyRejectedSelected || IsVacancyClosingSoonSelected || IsApplicationSubmittedSelected || IsVacancyRejectedByEmployerSelected;
    }
}