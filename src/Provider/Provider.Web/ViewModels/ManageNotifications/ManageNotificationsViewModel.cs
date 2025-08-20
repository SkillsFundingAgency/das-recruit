using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications
{
    public class ManageNotificationsViewModel
    {
        public bool IsVacancyRejectedSelected { get; set; }
        public bool IsVacancyRejectedByEmployerSelected { get; set; }
        public bool IsVacancyClosingSoonSelected { get; set; }
        public bool IsApplicationSubmittedSelected { get; set; }
        public NotificationFrequency? NotificationFrequency { get; set; }
        public NotificationScope? NotificationScope { get; set; }
        public bool HasAnySubscription => IsVacancyRejectedSelected || IsVacancyClosingSoonSelected || IsApplicationSubmittedSelected || IsVacancyRejectedByEmployerSelected;
        public long Ukprn { get; set; }
    }

    public class ManageNotificationsViewModelEx : ManageNotificationsViewModel
    {
        public bool Updated { get; set; }
        public string VacancyApprovedOrRejectedValue { get; init; }
        public string ApplicationSubmittedValue { get; init; }
        public string ApplicationSubmittedFrequencyValue { get; init; }
        public string SharedApplicationReviewedValue { get; init; }
        public string ProviderAttachedToVacancyValue { get; init; }
    }
}