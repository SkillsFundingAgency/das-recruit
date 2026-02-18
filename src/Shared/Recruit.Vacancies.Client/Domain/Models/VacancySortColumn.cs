using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VacancySortColumn
{
    CreatedDate, // default sort column
    ClosingDate, 
}