using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;

public class ManageNotificationsEditModel : ManageNotificationsRouteModel
{
    public bool IsVacancyRejectedSelected { get; set; }
    public bool IsVacancySentForEmployerReviewSelected { get; set; }
    public bool IsVacancyClosingSoonSelected { get; set; }
    public bool IsApplicationSubmittedSelected { get; set; }
    public NotificationFrequency? NotificationFrequency { get; set; }
    public NotificationScope? NotificationScope { get; set; }
    public bool HasAnySubscription => IsVacancyRejectedSelected || IsVacancyClosingSoonSelected || IsApplicationSubmittedSelected || IsVacancySentForEmployerReviewSelected;
}

public class ManageNotificationsEditModelEx : ManageNotificationsEditModel
{
    public string VacancyApprovedOrRejectedValue { get; init; }
    public string ApplicationSubmittedValue { get; init; }
    public string ApplicationSubmittedFrequencyValue { get; init; }
}