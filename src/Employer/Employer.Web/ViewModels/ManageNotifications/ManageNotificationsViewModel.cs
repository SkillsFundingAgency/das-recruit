using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications
{
    public class ManageNotificationsViewModel : ManageNotificationsRouteModel
    {
        public bool IsVacancyRejectedSelected { get; set; }
        public bool IsVacancySentForEmployerReviewSelected { get; set; }
        public bool IsVacancyClosingSoonSelected { get; set; }
        public bool IsApplicationSubmittedSelected { get; set; }
        public NotificationFrequency? NotificationFrequency { get; set; }
        public NotificationScope? NotificationScope { get; set; }
        public bool HasAnySubscription => IsVacancyRejectedSelected || IsVacancyClosingSoonSelected || IsApplicationSubmittedSelected || IsVacancySentForEmployerReviewSelected;     
        public bool EnvironmentIsProd { get; set; }
    }
    
    public class ManageNotificationsViewModelEx : ManageNotificationsRouteModel
    {
        public bool Updated { get; set; }
        public string VacancyApprovedOrRejectedOptionValue { get; init; }
        public string ApplicationSubmittedOptionValue { get; init; }
        public string ApplicationSubmittedFrequencyMineOptionValue { get; init; }
        public string ApplicationSubmittedFrequencyAllOptionValue { get; init; }
    }
}