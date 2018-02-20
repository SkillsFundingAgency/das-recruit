using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Domain.Entities;

namespace Esfa.Recruit.Storage.Client.Domain.QueryStore
{
    public interface IQueryStoreReader
    {
        Task<Vacancy> GetVacancyForEditAsync(Guid vacancyId);
    }
}