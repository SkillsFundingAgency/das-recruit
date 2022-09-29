using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DeleteVacancyOrchestrator
    {
        private readonly IEmployerVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;

        public DeleteVacancyOrchestrator(IEmployerVacancyClient client, IRecruitVacancyClient vacancyClient, IUtility utility)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _utility = utility;
        }

        public async Task<DeleteViewModel> GetDeleteViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(vrm.VacancyId);

            _utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            var vm = new DeleteViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                Title = vacancy.Title,
                Status = vacancy.Status
            };

            return vm;
        }

        public async Task DeleteVacancyAsync(DeleteEditModel m, VacancyUser user)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(m.VacancyId);

            _utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            await _client.DeleteVacancyAsync(vacancy.Id, user);
        }
    }
}
