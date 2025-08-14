using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public enum NotificationScope
{
    UserSubmittedVacancies,
    OrganisationVacancies,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public enum NotificationScopeEx
{
    UserSubmittedVacancies,
    OrganisationVacancies,
    Default,
}