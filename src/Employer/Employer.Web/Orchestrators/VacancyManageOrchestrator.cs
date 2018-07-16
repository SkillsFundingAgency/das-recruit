using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class VacancyManageOrchestrator
    {
        private readonly DisplayVacancyViewModelMapper _vacancyDisplayMapper;
        private readonly IEmployerVacancyClient _client;

        public VacancyManageOrchestrator(DisplayVacancyViewModelMapper vacancyDisplayMapper, IEmployerVacancyClient client)
        {
            _vacancyDisplayMapper = vacancyDisplayMapper;
            _client = client;
        }

        public async Task<Vacancy> GetVacancy(VacancyRouteModel vrm)
        {
            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            Utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            return vacancy;
        }
            submittedViewModel.SubmittedDate = vacancy.SubmittedDate.Value.AsGdsDate();
            approvedViewModel.ApprovedDate = vacancy.ApprovedDate.Value.AsGdsDate();
            closedViewModel.ClosedDate = vacancy.ClosedDate.Value.AsGdsDate();
    }
}
