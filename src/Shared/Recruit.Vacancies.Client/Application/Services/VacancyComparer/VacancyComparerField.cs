namespace Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;

public class VacancyComparerField
{

    public VacancyComparerField(string fieldName, bool areEqual)
    {
        FieldName = fieldName;
        AreEqual = areEqual;
    }

    public string FieldName { get; }
    public bool AreEqual { get; }
}