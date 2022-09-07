using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

namespace Esfa.Recruit.Provider.Web.Services
{
    public interface IProviderAlertsViewModelFactory
    {
        Task<AlertsViewModel> Create(User user);
        AlertsViewModel Create(ProviderDashboard providerDashboard, User user);
    }
}
