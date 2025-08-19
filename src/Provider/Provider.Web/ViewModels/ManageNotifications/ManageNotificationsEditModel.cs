using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;

public class ManageNotificationsEditModel
{
    [FromRoute]
    public long Ukprn { get; set; }
    public bool IsVacancyRejectedSelected { get; set; }
    public bool IsVacancyRejectedByEmployerSelected { get; set; }
    public bool IsVacancyClosingSoonSelected { get; set; }
    public bool IsApplicationSubmittedSelected { get; set; }
    public NotificationFrequency? NotificationFrequency { get; set; }
    public NotificationScope? NotificationScope { get; set; }
    public bool HasAnySubscription => IsVacancyRejectedSelected || IsVacancyClosingSoonSelected || IsApplicationSubmittedSelected || IsVacancyRejectedByEmployerSelected;
}

public class ManageNotificationsEditModelEx : ManageNotificationsEditModel
{
    public string VacancyApprovedOrRejectedOptionValue { get; init; }
    public string ApplicationSubmittedOptionValue { get; init; }
    public string ApplicationSubmittedFrequencyMineOptionValue { get; init; }
    public string ApplicationSubmittedFrequencyAllOptionValue { get; init; }
    public string SharedApplicationReviewedOptionValue { get; init; }
    public string ProviderAttachedToVacancyOptionValue { get; init; }
}