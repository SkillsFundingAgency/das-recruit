using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ColumnSortOrder
{
    Asc,
    Desc,
}