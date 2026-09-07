using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArchiveType
{
    Auto = 0,
    Manual = 1
}