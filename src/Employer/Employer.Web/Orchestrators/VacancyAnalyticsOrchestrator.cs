using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IVacancyAnalyticsOrchestrator
    {
        Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel vrm);
    }

    public class VacancyAnalyticsOrchestrator : IVacancyAnalyticsOrchestrator
    {
        private readonly IRecruitVacancyClient _client;
        private readonly EmployerRecruitSystemConfiguration _systemConfig;
        private readonly IUtility _utility;

        public VacancyAnalyticsOrchestrator(IRecruitVacancyClient client, EmployerRecruitSystemConfiguration systemConfig, IUtility utility)
        {
            _client = client;
            _systemConfig = systemConfig;
            _utility = utility;
        }

        public async Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel vrm)
        {
            var viewModel = new VacancyAnalyticsViewModel();

            var vacancy = await _client.GetVacancyAsync(vrm.VacancyId);

            _utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId, false);

            var vacancyAnalyticsTask = await _client.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.Value);
            var analyticsSummary = vacancyAnalyticsTask ?? new VacancyAnalyticsSummary();
            viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());
            viewModel.AnalyticsAvailableAfterApprovalDate = _systemConfig.ShowAnalyticsForVacanciesApprovedAfterDate.AsGdsDate();
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.VacancyReference = vacancy.VacancyReference.Value;
            viewModel.VacancyId = vacancy.Id;
            viewModel.EmployerAccountId = vrm.EmployerAccountId;

            return viewModel;
        }
    }
}
