namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class ManualQaFieldEditIndicator
{
    public string FieldIdentifier { get; set; }
    public string BeforeEdit { get; set; }
    public string AfterEdit { get; set; }
}