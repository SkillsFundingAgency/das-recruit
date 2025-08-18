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
        public string VacancyApprovedOrRejectedOptionValue { get; init; }
        public string ApplicationSubmittedOptionValue { get; init; }
        public string ApplicationSubmittedFrequencyMineOptionValue { get; init; }
        public string ApplicationSubmittedFrequencyAllOptionValue { get; init; }
        public string SharedApplicationReviewedOptionValue { get; init; }
        public string ProviderAttachedToVacancyOptionValue { get; init; }
    }
}