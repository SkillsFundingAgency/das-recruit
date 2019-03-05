using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ProviderApplicationsReportOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ProviderApplicationsReportOrchestrator(IRecruitVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public ProviderApplicationsReportCreateViewModel GetCreateViewModel()
        {
            return new ProviderApplicationsReportCreateViewModel { };
        }

        public ProviderApplicationsReportCreateViewModel GetCreateViewModel(ProviderApplicationsReportCreateEditModel model)
        {
            var vm = GetCreateViewModel();

            vm.DateRange = model.DateRange;
            vm.FromDay = model.FromDay;
            vm.FromMonth = model.FromMonth;
            vm.FromYear = model.FromYear;
            vm.ToDay = model.ToDay;
            vm.ToMonth = model.ToMonth;
            vm.ToYear = model.ToYear;

            return vm;
        }
    }
}
