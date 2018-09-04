using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;

        public DashboardOrchestrator(IQaVacancyClient vacancyClient, ITimeProvider timeProvider)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string searchTerm, VacancyUser vacancyUser)
        {            
            var reviews = await _vacancyClient.GetDashboardAsync();
            var vm = MapToViewModel(reviews);

            if(string.IsNullOrEmpty(searchTerm)) return vm;

            vm.LastSearchTerm = searchTerm;
            var result = await _vacancyClient.GetSearchResultsAsync(searchTerm);
            vm.SearchResults = result.Select(v => MapToViewModel(v, vacancyUser)).ToList();
            return vm;
        }

        private VacancyReviewSearchModel MapToViewModel(QaVacancySummary qaVacancySummary, VacancyUser vacancyUser)
        {
            return new VacancyReviewSearchModel()
            {
                AssignedTo = qaVacancySummary.ReviewAssignedToUserName,
                AssignedTimeElapsed = GetElapsedTime(qaVacancySummary.ReviewStartedOn),
                ClosingDate = qaVacancySummary.ClosingDate.ToLocalTime(),
                EmployerName = qaVacancySummary.EmployerName,
                VacancyReference = $"VAC{qaVacancySummary.VacancyReference}",
                VacancyTitle = qaVacancySummary.Title,
                ReviewId = qaVacancySummary.Id,
                SubmittedDate = qaVacancySummary.SubmittedDate.ToLocalTime(),
                IsAvailableForReview = string.IsNullOrEmpty(qaVacancySummary.ReviewAssignedToUserId) || qaVacancySummary.ReviewAssignedToUserId == vacancyUser.UserId,
                IsNotAvailableForReview = !string.IsNullOrEmpty(qaVacancySummary.ReviewAssignedToUserId) && qaVacancySummary.ReviewAssignedToUserId != vacancyUser.UserId
            };
        }

        private string GetElapsedTime(DateTime? value)
        {
            if (value == null) return string.Empty;
            var diff = _timeProvider.Now - value.Value;

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