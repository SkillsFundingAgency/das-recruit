using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
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
            var reviews = await _vacancyClient.GetDashboardAsync().ConfigureAwait(false);
            var vm = MapToViewModel(reviews);            
            return vm;
        }

        public async Task<List<VacancyReviewSearchModel>> GetSearchResultsAsync(string searchTerm)
        {
            var result = await _vacancyClient.GetSearchResultsAsync(searchTerm).ConfigureAwait(false);
            return result.Select(MapToViewModel).ToList();
        }

        private VacancyReviewSearchModel MapToViewModel(VacancyReviewSearch vacancyReviewSearch)
        {
            return new VacancyReviewSearchModel()
            {
                AssignedTo = vacancyReviewSearch.ReviewAssignedTo,
                AssignedTimeElapsed = GetElapsedTime(vacancyReviewSearch.ReviewStartedOn?.ToLocalTime()),
                ClosingDate = vacancyReviewSearch.ClosingDate.ToLocalTime(),
                EmployerName = vacancyReviewSearch.EmployerName,
                VacancyReference = $"VAC{vacancyReviewSearch.VacancyReference}",
                VacancyTitle = vacancyReviewSearch.Title,
                ReviewId = vacancyReviewSearch.Id,
                SubmittedDate = vacancyReviewSearch.SubmittedDate.ToLocalTime()
            };
        }

        private static string GetElapsedTime(DateTime? value)
        {
            if (value == null) return string.Empty;
            var diff = DateTime.Now - value.Value;

            var hours = diff.Hours > 0 ? $"{diff.Hours}h " : string.Empty;
            var minutes = diff.Minutes > 0 ? $"{diff.Minutes}m" : string.Empty;

            return hours + minutes;
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