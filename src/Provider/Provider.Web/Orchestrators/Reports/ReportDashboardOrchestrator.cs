using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportDashboard;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ReportDashboardOrchestrator
    {
        private readonly IProviderVacancyClient _vacancyClient;

        public ReportDashboardOrchestrator(IProviderVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<ReportsDashboardViewModel> GetDashboardViewModel(long ukprn)
        {
            var reports = await _vacancyClient.GetReportsForProviderAsync(ukprn);

            var vm = new ReportsDashboardViewModel 
            {
                ProcessingCount = reports.Count(r => r.IsProcessing),
                Reports = reports.Select(r => new ReportRowViewModel 
                {
                    FromDate = ((DateTime)r.Parameters.Single(p => p.Name == ReportParameterName.FromDate).Value).AsGdsDate(),
                    ToDate = ((DateTime)r.Parameters.Single(p => p.Name == ReportParameterName.ToDate).Value).AsGdsDate(),
                    DownloadCount = r.DownloadCount,
                    CreatedDate = r.RequestedOn.AsGdsDateTime(),
                    CreatedBy = r.RequestedBy.Name,
                    Status = r.Status,
                    IsProcessing = r.IsProcessing
                })
            };

            return vm;
        }

        
    }
}
