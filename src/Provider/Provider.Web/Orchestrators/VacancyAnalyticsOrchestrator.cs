using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public interface IVacancyAnalyticsOrchestrator
    {
        Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel routeModel);
    }

    public class VacancyAnalyticsOrchestrator(IRecruitVacancyClient client) : IVacancyAnalyticsOrchestrator
    {
        public async Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel routeModel)
        {
            var viewModel = new VacancyAnalyticsViewModel();
            var vacancy = await client.GetVacancyAsync((System.Guid)routeModel.VacancyId!);
            var vacancyAnalyticsTask = await client.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.GetValueOrDefault());
            var analyticsSummary = vacancyAnalyticsTask ?? new VacancyAnalyticsSummary();
            viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());

            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.VacancyReference = vacancy.VacancyReference.GetValueOrDefault();
            viewModel.VacancyId = routeModel.VacancyId;
            viewModel.Ukprn = routeModel.Ukprn;

            return viewModel;
        }
    }
}
