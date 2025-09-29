using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Provider.Web.ViewModels.Dashboard;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class DashboardOrchestrator(
        IProviderVacancyClient vacancyClient,
        IRecruitVacancyClient client,
        IProviderRelationshipsService providerRelationshipsService)
    {
        public virtual async Task<DashboardViewModel> GetDashboardViewModelAsync(VacancyUser user)
        {
            long ukprn = user.Ukprn ?? 0;

            await client.UserSignedInAsync(user, UserType.Provider);
            var dashboardTask = vacancyClient.GetDashboardSummary(ukprn, user.UserId);
            var providerTask = providerRelationshipsService.CheckProviderHasPermissions(ukprn, OperationType.RecruitmentRequiresReview);
            
            await Task.WhenAll(dashboardTask, providerTask);
            
            var dashboard = dashboardTask.Result;
            bool providerPermissions = providerTask.Result;
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
                HasEmployerReviewPermission = providerPermissions,
                Ukprn = ukprn
            };
        }
    }
}