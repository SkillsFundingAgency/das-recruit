using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Application.Commands;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class CloseVacancyOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IMessaging _messaging;

        public CloseVacancyOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, IMessaging messaging)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _messaging = messaging;
        }

        public async Task<CloseViewModel> GetCloseViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);

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
            var vacancy = await _vacancyClient.GetVacancyAsync(m.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanClose)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForClosing, vacancy.Title));

             var command = new CloseVacancyCommand(m.VacancyId, user, ClosureReason.Manual);

            await _messaging.SendCommandAsync(command);

            return new OrchestratorResponse<VacancyInfo>(new VacancyInfo
            {
                Id = vacancy.Id,
                VacancyReference = vacancy.VacancyReference.Value.ToString(),
                Title = vacancy.Title
            });
        }
    }
}
