namespace Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;

public class VacancyComparerField(string fieldName, bool areEqual)
{
    public string FieldName { get; } = fieldName;
    public bool AreEqual { get; } = areEqual;
}