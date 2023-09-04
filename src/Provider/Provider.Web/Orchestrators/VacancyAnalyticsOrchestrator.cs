using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public interface IVacancyAnalyticsOrchestrator
    {
        Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel routeModel);
    }

    public class VacancyAnalyticsOrchestrator : IVacancyAnalyticsOrchestrator
    {
        private readonly IRecruitVacancyClient _client;
        private readonly ProviderRecruitSystemConfiguration _systemConfig;

        public VacancyAnalyticsOrchestrator(IRecruitVacancyClient client, ProviderRecruitSystemConfiguration systemConfig)
        {
            _client = client;
            _systemConfig = systemConfig;
        }

        public async Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel routeModel)
        {
            var viewModel = new VacancyAnalyticsViewModel();
            var vacancy = await _client.GetVacancyAsync((System.Guid)routeModel.VacancyId);
            var vacancyAnalyticsTask = await _client.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.Value);
            var analyticsSummary = vacancyAnalyticsTask ?? new VacancyAnalyticsSummary();
            viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());

            viewModel.AnalyticsAvailableAfterApprovalDate = _systemConfig.ShowAnalyticsForVacanciesApprovedAfterDate.AsGdsDate();
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.VacancyReference = vacancy.VacancyReference.Value;
            viewModel.VacancyId = routeModel.VacancyId;
            viewModel.Ukprn = routeModel.Ukprn;

            return viewModel;
        }
    }
}
