using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Configuration
{
    public class ServiceParameters
    {
        public ServiceParameters(string serviceType)
        {
            VacancyType = Enum.TryParse<VacancyType>(serviceType, true, out var type)
                ? type
                : Domain.Entities.VacancyType.Apprenticeship;
        }

        public VacancyType? VacancyType { get; set; }
    }

}