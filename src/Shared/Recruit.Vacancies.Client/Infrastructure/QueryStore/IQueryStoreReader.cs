using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;

public interface IQueryStoreReader
{
    Task<IEnumerable<LiveVacancy>> GetLiveExpiredVacancies(DateTime closingDate);
    Task<ClosedVacancy> GetClosedVacancy(long vacancyReference);
}