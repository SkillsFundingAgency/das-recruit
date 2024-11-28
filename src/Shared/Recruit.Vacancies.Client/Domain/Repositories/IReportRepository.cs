using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IReportRepository
    {
        Task CreateAsync(Report report);
        Task UpdateAsync(Report report);
        Task<List<T>> GetReportsForProviderAsync<T>(long ukprn);
        Task<Report> GetReportAsync(Guid reportId);
        Task<int> DeleteReportsCreatedBeforeAsync(DateTime requestedOn);
        Task IncrementReportDownloadCountAsync(Guid reportId);
        Task<List<T>> GetReportsForQaAsync<T>();
    }
}
