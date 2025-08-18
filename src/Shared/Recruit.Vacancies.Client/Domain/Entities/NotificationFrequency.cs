using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public enum NotificationFrequency
{
    Immediately,
    Daily,
    Weekly,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public enum NotificationFrequencyEx
{
    NotSet,
    Never,
    Immediately,
    Daily,
    Weekly,
}