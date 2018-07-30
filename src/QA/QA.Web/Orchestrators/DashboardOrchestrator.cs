using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;

        public DashboardOrchestrator(IQaVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync()
        {            
            var reviews = await _vacancyClient.GetDashboardAsync();
            var vm = MapToViewModel(reviews);
            return vm;
        }

        private static DashboardViewModel MapToViewModel(QaDashboard dashboard)
        {
            var vm = new DashboardViewModel
            {
                TotalVacanciesForReview = dashboard.TotalVacanciesForReview,
                TotalVacanciesBrokenSla = dashboard.TotalVacanciesBrokenSla,
                TotalVacanciesResubmitted = dashboard.TotalVacanciesResubmitted,

                //todo: this will be deleted
                AllReviews = dashboard.AllReviews.Select(r => new ReviewDashboardItem
                {
                    ReviewId = r.Id,
                    VacancyReference = r.VacancyReference,
                    Title = r.Title,
                    Status = CalculateStatus(r),
                    IsReferred = r.ManualOutcome == ManualQaOutcome.Referred
                }).ToList()
            };

            return vm;
        }

        private static string CalculateStatus(VacancyReview review)
        {
            if (review.Status == ReviewStatus.UnderReview && review.ManualOutcome == ManualQaOutcome.Referred)
            {
                return review.Status.GetDisplayName();
            }
            
            return VacancyStatus.Submitted.GetDisplayName();
        }
    }
}