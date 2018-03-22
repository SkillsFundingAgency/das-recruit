using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    public interface IVacancyValidator
    {
        void ValidateAndThrow(Vacancy vacancy, VacancyRuleSet validationsToRun);
    }
}