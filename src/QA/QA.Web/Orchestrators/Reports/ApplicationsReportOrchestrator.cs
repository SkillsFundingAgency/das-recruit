using System;
using System.Globalization;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels.Reports;
using Esfa.Recruit.Qa.Web.ViewModels.Reports.ApplicationsReport;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Qa.Web.Orchestrators.Reports
{
    public class ApplicationsReportOrchestrator
    {
        private readonly IQaVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public ApplicationsReportOrchestrator(IQaVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public ApplicationsReportCreateViewModel GetCreateViewModel()
        {
            return new ApplicationsReportCreateViewModel();
        }

        public ApplicationsReportCreateViewModel GetCreateViewModel(ApplicationsReportCreateEditModel model)
        {
            var vm = GetCreateViewModel();

            vm.DateRange = model.DateRange;
            vm.FromDay = model.FromDay;
            vm.FromMonth = model.FromMonth;
            vm.FromYear = model.FromYear;
            vm.ToDay = model.ToDay;
            vm.ToMonth = model.ToMonth;
            vm.ToYear = model.ToYear;

            return vm;
        }

        public Task<Guid> PostCreateViewModelAsync(ApplicationsReportCreateEditModel model, VacancyUser user)
        {
            DateTime toDateInclusive = _timeProvider.NextDay.AddTicks(-1);
            DateTime fromDate;

            switch (model.DateRange)
            {
                case DateRangeType.Last7Days:
                    fromDate = _timeProvider.Today.AddDays(-7);
                    break;
                case DateRangeType.Last14Days:
                    fromDate = _timeProvider.Today.AddDays(-14);
                    break;
                case DateRangeType.Last30Days:
                    fromDate = _timeProvider.Today.AddDays(-30);
                    break;
                case DateRangeType.Custom:
                    fromDate = model.FromDate.AsDateTimeUk().Value.Date.ToUniversalTime();
                    toDateInclusive = model.ToDate.AsDateTimeUk().Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                    break;
                default:
                    throw new Exception($"Cannot handle this date range type:{model.DateRange.ToString()}");
            }

            var reportName = $"{fromDate.ToUkTime().AsGdsDate()} to {toDateInclusive.ToUkTime().AsGdsDate()}";

            return _client.CreateApplicationsReportAsync(fromDate, toDateInclusive, user, reportName);
        }
    }
}
