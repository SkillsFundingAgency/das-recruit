using System.Threading.Tasks;

namespace SFA.DAS.Recruit.Api.Services
{
    public interface IQueryStoreReader
    {
        Task<EmployerDashboard> GetEmployerDashboardAsync(string employerAccountId);
        Task<VacancyApplications> GetVacancyApplicationsAsync(string vacancyReference);
        Task<ProviderDashboard> GetProviderDashboardAsync(long ukprn);
        Task<BlockedProviderOrganisations> GetBlockedProviders();
        Task<BlockedEmployerOrganisations> GetBlockedEmployers();
    }
}