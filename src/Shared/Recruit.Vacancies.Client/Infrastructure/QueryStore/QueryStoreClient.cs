using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore
{
    public class QueryStoreClient(IQueryStore queryStore, ITimeProvider timeProvider)
        : IQueryStoreReader, IQueryStoreWriter
    {
        public Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities)
        {
            var employerVacancyDataItem = new EmployerEditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(employerAccountId),
                LegalEntities = legalEntities,
                LastUpdated = timeProvider.Now
            };

            return queryStore.UpsertAsync(employerVacancyDataItem);
        }

        public Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers, bool hasAgreement)
        {
            var providerVacancyDataItem = new ProviderEditVacancyInfo
            {
                Id = QueryViewType.EditVacancyInfo.GetIdValue(ukprn),
                Employers = employers,
                HasProviderAgreement = hasAgreement,
                LastUpdated = timeProvider.Now
            };

            return queryStore.UpsertAsync(providerVacancyDataItem);
        }

        public Task DeleteLiveVacancyAsync(long vacancyReference)
        {
            var liveVacancyId = GetLiveVacancyId(vacancyReference);
            return queryStore.DeleteAsync<LiveVacancy>(QueryViewType.LiveVacancy.TypeName, liveVacancyId);
        }

        public Task<ClosedVacancy> GetClosedVacancy(long vacancyReference)
        {
            var key = QueryViewType.ClosedVacancy.GetIdValue(vacancyReference.ToString());
            return queryStore.GetAsync<ClosedVacancy>(QueryViewType.ClosedVacancy.TypeName, key);
        }

        public Task<IEnumerable<LiveVacancy>> GetLiveExpiredVacancies(DateTime closingDate)
        {
            return queryStore.GetAllLiveExpired(closingDate);
        }

        public Task UpdateClosedVacancyAsync(ClosedVacancy closedVacancy)
        {
            return queryStore.UpsertAsync(closedVacancy);
        }

        
        private string GetLiveVacancyId(long vacancyReference)
        {
            return QueryViewType.LiveVacancy.GetIdValue(vacancyReference.ToString());
        }
    }
}