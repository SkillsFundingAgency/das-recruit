using Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DeleteVacancyOrchestrator(
        IEmployerVacancyClient client,
        IRecruitVacancyClient vacancyClient,
        IUtility utility)
    {
        public async Task<DeleteViewModel> GetDeleteViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await vacancyClient.GetVacancyAsync(vrm.VacancyId);

            utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

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

        public async Task<Vacancy> DeleteVacancyAsync(DeleteEditModel m, VacancyUser user)
        {
            var vacancy = await vacancyClient.GetVacancyAsync(m.VacancyId);

            utility.CheckAuthorisedAccess(vacancy, m.EmployerAccountId);

            if (!vacancy.CanDelete)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));

            await client.DeleteVacancyAsync(vacancy.Id, user);
            return vacancy;
        }
    }
}
