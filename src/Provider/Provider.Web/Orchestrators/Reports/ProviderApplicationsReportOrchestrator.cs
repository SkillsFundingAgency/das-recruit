using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ProviderApplicationsReportOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public ProviderApplicationsReportOrchestrator(IProviderVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public ProviderApplicationsReportCreateViewModel GetCreateViewModel()
        {
            return new ProviderApplicationsReportCreateViewModel();
        }

        public ProviderApplicationsReportCreateViewModel GetCreateViewModel(ProviderApplicationsReportCreateEditModel model)
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

        public Task<Guid> PostCreateViewModelAsync(ProviderApplicationsReportCreateEditModel model, VacancyUser user)
        {
            DateTime toDateInclusive = _timeProvider.NextDay.AddTicks(-1);
            DateTime fromDate;

            switch (model.DateRange)
            {
                case DateRangeType.Last7Days:
                    fromDate = toDateInclusive.Date.AddDays(-7);
                    break;
                case DateRangeType.Last14Days:
                    fromDate = toDateInclusive.Date.AddDays(-14);
                    break;
                case DateRangeType.Last30Days:
                    fromDate = toDateInclusive.Date.AddDays(-30);
                    break;
                case DateRangeType.Custom:
                    fromDate = model.FromDate.AsDateTimeUk().Value.ToUniversalTime();
                    toDateInclusive = model.ToDate.AsDateTimeUk().Value.ToUniversalTime().AddDays(1).AddTicks(-1);
                    break;
                default:
                    throw new Exception($"Cannot handle this date range type:{model.DateRange.ToString()}");
            }

            var reportName = $"{fromDate.AsGdsDate()} to {toDateInclusive.AsGdsDate()}";

            return _client.CreateProviderApplicationsReportAsync(model.Ukprn, fromDate, toDateInclusive, user, reportName);
        }
    }
}
