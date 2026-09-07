using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Services;

public class ProviderAlertsViewModelFactory(
    AlertViewModelService alertViewModelService,
    IProviderVacancyClient providerVacancyClient)
    : IProviderAlertsViewModelFactory
{
    public async Task<AlertsViewModel> Create(User user)
    {
        if (user?.Ukprn == null)
            throw new ArgumentNullException(nameof(user));

        var vacancies = await providerVacancyClient.GetDashboardAsync(user.Ukprn.Value, "", 1, 25, "", "", FilteringOptions.Closed, null);

        var transferredVacanciesAlert = new ProviderTransferredVacanciesAlertViewModel
        {
            LegalEntityNames = vacancies.ProviderTransferredVacanciesAlert.LegalEntityNames
        };
            
        var withdrawnByQaVacanciesAlert = alertViewModelService.GetWithdrawnByQaVacanciesAlert(
            vacancies.Vacancies,
            user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

        if (user.Ukprn.HasValue)
        {
            if (transferredVacanciesAlert is not null)
            {
                transferredVacanciesAlert.Ukprn = user.Ukprn.Value;    
            }
            if (withdrawnByQaVacanciesAlert != null)
            {
                withdrawnByQaVacanciesAlert.Ukprn = user.Ukprn.Value;    
            }
        }
            
        return new AlertsViewModel(transferredVacanciesAlert, withdrawnByQaVacanciesAlert, user.Ukprn);
    }
    public AlertsViewModel Create(ProviderDashboard providerDashboard, User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var transferredVacanciesAlert = new ProviderTransferredVacanciesAlertViewModel
        {
            LegalEntityNames = providerDashboard.ProviderTransferredVacanciesAlert?.LegalEntityNames ?? [],
        };

        var withdrawnByQaVacanciesAlert = alertViewModelService.GetWithdrawnByQaVacanciesAlert(
            providerDashboard?.Vacancies ?? Array.Empty<VacancySummary>(),
            user.ClosedVacanciesWithdrawnByQaAlertDismissedOn);

        if (user.Ukprn.HasValue)
        {
            if (transferredVacanciesAlert is not null)
            {
                transferredVacanciesAlert.Ukprn = user.Ukprn.Value;    
            }
            if (withdrawnByQaVacanciesAlert != null)
            {
                withdrawnByQaVacanciesAlert.Ukprn = user.Ukprn.Value;    
            }
        }
            
        return new AlertsViewModel(transferredVacanciesAlert, withdrawnByQaVacanciesAlert, user.Ukprn);
    }
}