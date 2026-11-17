using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class VacancyIdentifier
{
    public Guid Id { get; set; }
    public long? VacancyReference { get; set; }
    public VacancyStatus Status { get; set; }
    public DateTime? ClosingDate { get; set; }
}