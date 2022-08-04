using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class FutureProspectsOrchestrator : IFutureProspectsOrchestrator
    {
        public async Task<FutureProspectsViewModel> GetViewModel(VacancyRouteModel vrm)
        {
            throw new System.NotImplementedException();
        }

        public async Task<OrchestratorResponse> PostEditModel(FutureProspectsEditModel futureProspectsEditModel, VacancyUser toVacancyUser)
        {
            throw new System.NotImplementedException();
        }
    }
}