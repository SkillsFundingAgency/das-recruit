using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator(IProviderVacancyClient vacancyClient,
        IRecruitVacancyClient client)
    {
        public virtual async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            long ukprn = user.Ukprn ?? 0;

            await client.UserSignedInAsync(user, UserType.Provider);
            var dashboard = await vacancyClient.GetDashboardSummary(ukprn, user.UserId);
            var alerts = new AlertsViewModel(new ProviderTransferredVacanciesAlertViewModel
            {
                LegalEntityNames = dashboard.ProviderTransferredVacanciesAlert.LegalEntityNames,
                Ukprn = ukprn
            }, new WithdrawnVacanciesAlertViewModel
            {
                ClosedVacancies = dashboard.WithdrawnVacanciesAlert.ClosedVacancies,
                Ukprn = ukprn
            }, ukprn);

            return new DashboardViewModel
            {
                ProviderDashboardSummary = dashboard,
                Alerts = alerts,
                Ukprn = ukprn
            };
        }
    }
}