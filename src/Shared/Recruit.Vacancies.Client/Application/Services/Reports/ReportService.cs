using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services.Reports
{
    public class ReportService : IReportService
    {
        public Task GenerateReportAsync(Guid reportId)
        {
            return Task.CompletedTask;
        }
    }
}
