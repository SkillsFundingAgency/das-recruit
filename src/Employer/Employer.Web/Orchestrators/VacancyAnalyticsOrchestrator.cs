using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyAnalytics;
using Esfa.Recruit.Shared.Web.Mappers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IVacancyAnalyticsOrchestrator
    {
        Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel vrm);
    }

    public class VacancyAnalyticsOrchestrator(IRecruitVacancyClient client, IUtility utility)
        : IVacancyAnalyticsOrchestrator
    {
        public async Task<VacancyAnalyticsViewModel> GetVacancyAnalytics(VacancyRouteModel vrm)
        {
            var viewModel = new VacancyAnalyticsViewModel();

            var vacancy = await client.GetVacancyAsync(vrm.VacancyId);

            utility.CheckAuthorisedAccess(vacancy, vrm.EmployerAccountId);

            var vacancyAnalyticsTask = await client.GetVacancyAnalyticsSummaryAsync(vacancy.VacancyReference.GetValueOrDefault());
            var analyticsSummary = vacancyAnalyticsTask ?? new VacancyAnalyticsSummary();
            viewModel.AnalyticsSummary = VacancyAnalyticsSummaryMapper.MapToVacancyAnalyticsSummaryViewModel(analyticsSummary, vacancy.LiveDate.GetValueOrDefault());
            viewModel.IsApplyThroughFaaVacancy = vacancy.ApplicationMethod == ApplicationMethod.ThroughFindAnApprenticeship;
            viewModel.VacancyReference = vacancy.VacancyReference.GetValueOrDefault();
            viewModel.VacancyId = vacancy.Id;
            viewModel.EmployerAccountId = vrm.EmployerAccountId;

            return viewModel;
        }
    }
}
