using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class EmployerService : IEmployerService
    {
        private readonly IEmployerProfileRepository _employerProfileRepository;

        public EmployerService(IEmployerProfileRepository employerProfileRepository)
        {
            _employerProfileRepository = employerProfileRepository;
        }

        public async Task<string> GetEmployerNameAsync(Vacancy vacancy)
        {
            if (!vacancy.CanEmployerEdit)
                return vacancy.EmployerName;

            if (vacancy.IsAnonymous)
                return vacancy.EmployerName;

            if (vacancy.EmployerNameOption == EmployerNameOption.TradingName) 
            {
                var profile = await _employerProfileRepository.GetAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
                return profile.TradingName;
            }

            return vacancy.LegalEntityName;
        }

        public async Task<string> GetEmployerDescriptionAsync(Vacancy vacancy)
        {
            if (!vacancy.CanEmployerEdit)
                return vacancy.EmployerDescription;

            var profile = await _employerProfileRepository.GetAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
            return profile?.AboutOrganisation ?? string.Empty;
        }
    }
}