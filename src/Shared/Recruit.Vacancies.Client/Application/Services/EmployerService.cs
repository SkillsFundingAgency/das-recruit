using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public class EmployerService(IEmployerProfileService employerProfileService) : IEmployerService
    {
        public async Task<string> GetEmployerNameAsync(Vacancy vacancy)
        {
            if (!vacancy.CanEmployerEdit)
                return vacancy.EmployerName;

            if (vacancy.IsAnonymous)
                return vacancy.EmployerName;

            if (vacancy.EmployerNameOption == EmployerNameOption.TradingName) 
            {
                var profile = await employerProfileService.GetAsync(vacancy.EmployerAccountId,
                    vacancy.AccountLegalEntityPublicHashedId);

                return profile.TradingName;
            }

            return vacancy.LegalEntityName;
        }

        public async Task<string> GetEmployerDescriptionAsync(Vacancy vacancy)
        {
            if (!vacancy.CanGetEmployerProfileAboutOrganisation)
                return vacancy.EmployerDescription;

            var profile = await employerProfileService.GetAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId);
            return profile?.AboutOrganisation ?? string.Empty;
        }
    }
}