using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public interface IQueryStoreWriter
    {
        Task UpdateEmployerDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries);
        Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities);
        Task UpdateLiveVacancyAsync(LiveVacancy vacancy);
        Task DeleteLiveVacancyAsync(long vacancyReference);
        Task RecreateLiveVacancies(IEnumerable<LiveVacancy> liveVacancies);
        Task UpdateVacancyApplicationsAsync(VacancyApplications vacancyApplications);
        Task UpdateQaDashboardAsync(QaDashboard qaDashboard);
        Task UpdateClosedVacancyAsync(ClosedVacancy closedVacancy);
    }
}