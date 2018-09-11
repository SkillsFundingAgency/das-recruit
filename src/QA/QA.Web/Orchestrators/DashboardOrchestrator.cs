using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;

namespace Esfa.Recruit.Qa.Web.Orchestrators
{
    public class DashboardOrchestrator
    {
        private readonly IQaVacancyClient _vacancyClient;
        private readonly ITimeProvider _timeProvider;
        private readonly UserAuthorizationService _userAuthorizationService;

        public DashboardOrchestrator(
            IQaVacancyClient vacancyClient, ITimeProvider timeProvider, UserAuthorizationService userAuthorizationService)
        {
            _vacancyClient = vacancyClient;
            _timeProvider = timeProvider;
            _userAuthorizationService = userAuthorizationService;
        }

        public async Task<DashboardViewModel> GetDashboardViewModelAsync(string searchTerm, ClaimsPrincipal user)
        {
            var vacancyUser = user.GetVacancyUser();
            var reviews = await _vacancyClient.GetDashboardAsync();
            var vm = MapToViewModel(reviews);

            vm.DisplayInProgressVacancies = await _userAuthorizationService.IsTeamLeadAsync(user);
            if (vm.DisplayInProgressVacancies)
            {
                var inProgressSummaries = await _vacancyClient.GetVacancyReviewsInProgressAsync();
                vm.InProgressVacancies = inProgressSummaries.Select(v => MapToViewModel(v, vacancyUser)).ToList();
            }

            if(string.IsNullOrEmpty(searchTerm)) return vm;

            vm.LastSearchTerm = searchTerm;
            var searchResults = await _vacancyClient.GetSearchResultsAsync(searchTerm);
            vm.SearchResults = searchResults.Select(v => MapToViewModel(v, vacancyUser)).ToList();
            return vm;
        }

        private VacancyReviewSearchModel MapToViewModel(QaVacancySummary qaVacancySummary, VacancyUser vacancyUser)
        {
            return new VacancyReviewSearchModel()
            {
                AssignedTo = qaVacancySummary.ReviewAssignedToUserName,
                AssignedTimeElapsedMessage = GetFormattedElapsedTime(qaVacancySummary.ReviewStartedOn),
                ClosingDate = qaVacancySummary.ClosingDate.ToLocalTime(),
                EmployerName = qaVacancySummary.EmployerName,
                VacancyReference = $"VAC{qaVacancySummary.VacancyReference}",
                VacancyTitle = qaVacancySummary.Title,
                ReviewId = qaVacancySummary.Id,
                SubmittedDate = qaVacancySummary.SubmittedDate.ToLocalTime(),
                IsAvailableForReview = qaVacancySummary.ReviewAssignedToUserId == vacancyUser.UserId ||
                                       _vacancyClient.VacancyReviewCanBeAssigned(qaVacancySummary.Status, qaVacancySummary.ReviewStartedOn)
            };
        }

        private string GetFormattedElapsedTime(DateTime? value)
        {
            if (value == null) return string.Empty;
            var diff = _timeProvider.Now - value.Value;

            if (diff < TimeSpan.FromMinutes(1))
                return "Being reviewed now.";

            var hours = diff.Hours > 0 ? $"{diff.Hours}h" : string.Empty;
            var minutes = diff.Minutes > 0 ? $"{diff.Minutes}m" : string.Empty;

            return $"Being reviewed for {hours} {minutes}.";
        }

        public async Task<Guid?> AssignNextVacancyReviewAsync(VacancyUser user)
        {
            await _vacancyClient.AssignNextVacancyReviewAsync(user);

            var userVacancyReviews = await _vacancyClient.GetAssignedVacancyReviewsForUserAsync(user.UserId);

            return userVacancyReviews.FirstOrDefault()?.Id;
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