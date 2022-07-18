using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    public class VacancyTypeRequirement : IAuthorizationRequirement
    {
        public VacancyType VacancyType { get; set; }

        public VacancyTypeRequirement(VacancyType vacancyType)
        {
            VacancyType = vacancyType;
        }
    }
}