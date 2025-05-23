﻿using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ReportConfirmation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ReportConfirmationOrchestrator : ReportOrchestratorBase
    {
        public ReportConfirmationOrchestrator(ILogger<ReportConfirmationOrchestrator> logger, IProviderVacancyClient client) : base(logger, client)
        {
        }

        public async Task<ConfirmationViewModel> GetConfirmationViewModelAsync(ReportRouteModel rrm)
        {
            var report = await GetReportAsync(rrm.Ukprn, rrm.ReportId);

            var vm = new ConfirmationViewModel 
            {
                FromDate = ((DateTime)report.Parameters[ReportParameterName.FromDate]).AsGdsDate(),
                ToDate = ((DateTime)report.Parameters[ReportParameterName.ToDate]).AsGdsDate(),
                Ukprn = rrm.Ukprn
            };

            return vm;
        }
    }
}
