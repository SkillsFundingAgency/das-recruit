using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.CloneVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class CloneVacancyOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        public CloneVacancyOrchestrator(IRecruitVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<CloneVacancyViewModel> GetCloneVacancyViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await GetVacancyAsync(vrm);

            if (!vacancy.CanClone)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForCloning, vacancy.Title));

            var vm = new CloneVacancyViewModel {
                StartDate = vacancy.StartDate?.AsGdsDate(),
                ClosingDate = vacancy.ClosingDate?.AsGdsDate()
            };

            return vm;
        }

        public async Task<Guid> CloneVacancy(VacancyRouteModel vrm, VacancyUser user)
        {
            var vacancy = await GetVacancyAsync(vrm);

            var clonedVacancyId = await _vacancyClient.CloneVacancyAsync(vacancy.Id, user, SourceOrigin.ProviderWeb);

            return clonedVacancyId;
        }

        private async Task<Vacancy> GetVacancyAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId.GetValueOrDefault());

            Utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            return vacancy;
        }
    }
}