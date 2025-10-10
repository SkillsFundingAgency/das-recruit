using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Employer.Web.ViewModels.Dashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class DashboardOrchestrator(
        IEmployerVacancyClient vacancyClient,
        IRecruitVacancyClient client,
        IProviderRelationshipsService providerRelationshipsService)
    {
        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string employerAccountId, VacancyUser user)
        {
            var dashboardTask = vacancyClient.GetDashboardSummary(employerAccountId, user.UserId);
            var userDetailsTask = client.GetUsersDetailsAsync(user.UserId);
            var providerTask = providerRelationshipsService.CheckEmployerHasPermissions(employerAccountId,
                    OperationType.RecruitmentRequiresReview);

            await Task.WhenAll(dashboardTask, userDetailsTask, providerTask);

            var dashboard = dashboardTask.Result;
            var userDetails = userDetailsTask.Result;

            if (userDetails == null)
            {
                var userOuter = await client.GetEmployerIdentifiersAsync(user.UserId, user.Email);
                user.Name = $"{userOuter.FirstName} {userOuter.LastName}";
                await client.UserSignedInAsync(user, UserType.Employer);
            }

            if (dashboard is {HasVacancies: false})
                await vacancyClient.SetupEmployerAsync(employerAccountId);

            bool providerPermissions = providerTask.Result;

            var employerRevokedTransfers = CreateTransferredAlert(dashboard.EmployerRevokedTransferredVacanciesAlert, employerAccountId);
            var blockedProviderTransferred = CreateTransferredAlert(dashboard.BlockedProviderTransferredVacanciesAlert, employerAccountId);

            var blockedProvider = dashboard.BlockedProviderAlert == null
                ? null
                : new BlockedProviderAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    ClosedVacancies = dashboard.BlockedProviderAlert.ClosedVacancies,
                    BlockedProviderNames = dashboard.BlockedProviderAlert.BlockedProviderNames
                };

            var withdrawn = dashboard.WithDrawnByQaVacanciesAlert == null
                ? null
                : new WithdrawnVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    ClosedVacancies = dashboard.WithDrawnByQaVacanciesAlert.ClosedVacancies,
                    Ukprn = user.Ukprn.GetValueOrDefault()
                };

            return new DashboardViewModel
            {
                EmployerDashboardSummary = dashboard,
                EmployerAccountId = employerAccountId,
                Alerts = new AlertsViewModel(employerRevokedTransfers, blockedProviderTransferred, blockedProvider, withdrawn),
                HasEmployerReviewPermission = providerPermissions
            };
        }

        private static EmployerTransferredVacanciesAlertViewModel CreateTransferredAlert(
            EmployerTransferredVacanciesAlertModel alertModel, string employerAccountId) =>
            alertModel == null
                ? null
                : new EmployerTransferredVacanciesAlertViewModel
                {
                    EmployerAccountId = employerAccountId,
                    TransferredVacanciesCount = alertModel.TransferredVacanciesCount,
                    TransferredVacanciesProviderNames = alertModel.TransferredVacanciesProviderNames
                };
    }
}