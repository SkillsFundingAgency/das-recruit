using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.Interfaces
{
    public interface IFutureProspectsOrchestrator
    {
        Task<FutureProspectsViewModel> GetViewModel(VacancyRouteModel vacancyRouteModel);
        Task<OrchestratorResponse> PostEditModel(FutureProspectsEditModel futureProspectsEditModel, VacancyUser vacancyUser);
    }
}