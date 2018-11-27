using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CloseVacancyOrchestrator
    {
        private readonly IEmployerVacancyClient _client;

        public CloseVacancyOrchestrator(IEmployerVacancyClient client)
        {
            _client = client;
        }

        public async Task<CloseViewModel> GetCloseViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanClose)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

            var vm = new CloseViewModel
            {
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference.Value.ToString()
            };

            return vm;
        }

        public async Task<OrchestratorResponse<VacancyInfo>> CloseVacancyAsync(CloseEditModel m, VacancyUser user)
        {
            var vacancy = await _client.GetVacancyAsync(m.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanClose)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

            await _client.CloseVacancyAsync(vacancy.Id, user);

            return new OrchestratorResponse<VacancyInfo>(new VacancyInfo
            {
                Id = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.Value.ToString(),
                Title = vacancy.Title
            });
        }
    }
}
