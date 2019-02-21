using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class EmployerOrchestrator 
    {
        private readonly IProviderVacancyClient _providerVacancyClient;

        public EmployerOrchestrator(IProviderVacancyClient providerVacancyClient)
        {
            _providerVacancyClient = providerVacancyClient;
        }

        public async Task<EmployersViewModel> GetEmployersViewModelAsync(VacancyRouteModel vacancyRouteModel)
        {
            var editVacancyInfo = await _providerVacancyClient.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn);

            var vm = new EmployersViewModel
            {
                Employers = editVacancyInfo.Employers.Select(e => new EmployerViewModel {Id = e.EmployerAccountId, Name = e.Name})
            };

            return vm;
        }
    }
}