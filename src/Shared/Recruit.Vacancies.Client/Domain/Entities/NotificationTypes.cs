using System;
using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

[Flags]
public enum NotificationTypes
{
    None = 0,
    VacancyRejected = 1,
    VacancyClosingSoon = 1 << 1,
    ApplicationSubmitted = 1 << 2,
    VacancySentForReview = 1 << 3,
    VacancyRejectedByEmployer = 1 << 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public enum NotificationTypesEx
{
    VacancyApprovedOrRejectedByDfE,
    VacancyClosingSoon,
    ApplicationSubmitted,
    VacancySentForReview,
    VacancyRejectedByEmployer,
}