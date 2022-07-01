using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Interfaces
{
    public interface ITaskListVacancy
    {
        string EmployerDescription { get; set; }
        ApplicationMethod? ApplicationMethod { get; set; }
        VacancyType? VacancyType { get; set; }
    }
}