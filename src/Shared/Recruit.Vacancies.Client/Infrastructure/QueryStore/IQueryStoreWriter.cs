using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreWriter
    {
        Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities);
        Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers, bool hasAgreement);
        Task UpdateLiveVacancyAsync(LiveVacancy vacancy);
        Task DeleteLiveVacancyAsync(long vacancyReference);
        Task UpdateVacancyApplicationsAsync(VacancyApplications vacancyApplications);
        Task UpdateClosedVacancyAsync(ClosedVacancy closedVacancy);
        Task UpdateBlockedProviders(IEnumerable<BlockedOrganisationSummary> blockedProviders);
        Task UpdateBlockedEmployers(IEnumerable<BlockedOrganisationSummary> blockedEmployers);
    }
}