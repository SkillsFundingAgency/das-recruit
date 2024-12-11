using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Configuration
{
    public class ServiceParameters
    {
        public VacancyType? VacancyType
        {
            get
            {
                return Domain.Entities.VacancyType.Apprenticeship;
            }
        }
    }
}