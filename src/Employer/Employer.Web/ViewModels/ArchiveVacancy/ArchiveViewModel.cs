namespace Esfa.Recruit.Employer.Web.ViewModels.ArchiveVacancy;

public class ArchiveViewModel : ArchiveEditModel
{
    public string Title { get; set; }
    public string EmployerName { get; set; }
    public long? VacancyReference { get; set; }
    public string VacancyReferenceDisplay => "VAC" + VacancyReference;
}