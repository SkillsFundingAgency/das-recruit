using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Alerts;
public record WithdrawnVacanciesAlertModel
{
    public List<string> ClosedVacancies { get; set; } = [];
}