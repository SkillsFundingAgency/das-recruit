using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;

public interface IVacancyComparerService
{
    VacancyComparerResult Compare(Vacancy a, Vacancy b);
}