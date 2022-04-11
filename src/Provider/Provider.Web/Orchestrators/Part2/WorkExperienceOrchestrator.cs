using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.WorkExperience;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class WorkExperienceOrchestrator
    {
        public async Task<WorkExperienceViewModel> GetWorkExperienceViewModelAsync(VacancyRouteModel vacancyRouteModel)
        {
            throw new System.NotImplementedException();
        }

        public async Task<OrchestratorResponse> PostWorkExperienceEditModelAsync(WorkExperienceEditModel editModel, VacancyUser vacancyUser)
        {
            throw new System.NotImplementedException();
        }
    }
}