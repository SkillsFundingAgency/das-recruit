using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DeleteVacancyOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;

        public DeleteVacancyOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<DeleteViewModel> GetDeleteViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId.Value);

            Utility.CheckAuthorisedAccess(vacancy, vrm.Ukprn);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new DeleteViewModel
            {
                Title = vacancy.Title
            };

            return vm;
        }

        public async Task DeleteVacancyAsync(DeleteEditModel m, VacancyUser user)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(m.VacancyId.Value);

            Utility.CheckAuthorisedAccess(vacancy, m.Ukprn);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            await _client.DeleteVacancyAsync(vacancy.Id, user);
        }
    }
}
