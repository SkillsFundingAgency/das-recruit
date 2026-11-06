namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class ReportOwner
{
    public ReportOwnerType OwnerType { get; set; }
    public string EmployerAccountId { get; set; }
    public long? Ukprn { get; set; }
}