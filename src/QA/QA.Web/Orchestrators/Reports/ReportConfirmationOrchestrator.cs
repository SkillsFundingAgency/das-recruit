using System;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.RouteModel;
using Esfa.Recruit.Qa.Web.ViewModels.Reports.ReportConfirmation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Qa.Web.Orchestrators.Reports
{
    public class ReportConfirmationOrchestrator : ReportOrchestratorBase
    {
        public ReportConfirmationOrchestrator(ILogger<ReportConfirmationOrchestrator> logger, IQaVacancyClient client) : base(logger, client)
        {
        }

        public async Task<ConfirmationViewModel> GetConfirmationViewModelAsync(ReportRouteModel rrm)
        {
            var report = await GetReportAsync(rrm.ReportId);

            var vm = new ConfirmationViewModel
            {
                FromDate = ((DateTime)report.Parameters[ReportParameterName.FromDate]).AsGdsDate(),
                ToDate = ((DateTime)report.Parameters[ReportParameterName.ToDate]).AsGdsDate(),
            };

            return vm;
        }
    }
}
