using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class EmployerNameService : IEmployerNameService
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IEmployerProfileRepository _employerProfileRepository;

        public EmployerNameService(IVacancyRepository vacancyRepository, IEmployerProfileRepository employerProfileRepository)
        {
            _vacancyRepository = vacancyRepository;
            _employerProfileRepository = employerProfileRepository;
        }

        public async Task<string> GetEmployerNameAsync(Guid vacancyId)
        {
            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyId);
            if(vacancy.CanEdit)
            {
                if (vacancy.EmployerNameOption == EmployerNameOption.TradingName) 
                {
                    var profile = await _employerProfileRepository.GetAsync(vacancy.EmployerAccountId, vacancy.LegalEntityId);
                    return profile.TradingName;
                }

                return vacancy.LegalEntityName;
            }
            
            return vacancy.EmployerName;
        }
    }
}