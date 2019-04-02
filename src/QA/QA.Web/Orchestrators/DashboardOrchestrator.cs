using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Esfa.Recruit.Qa.Web.Extensions;
using Esfa.Recruit.Qa.Web.Security;
using Esfa.Recruit.Qa.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

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

        private async Task<VacancyReviewSearchResultViewModel> MapToViewModel(VacancyReview vacancyReview, VacancyUser vacancyUser)
        {
            var isAvailableForReview =
                _vacancyClient.VacancyReviewCanBeAssigned(vacancyReview.Status, vacancyReview.ReviewedDate);

            var vacancy = await _vacancyClient.GetVacancyAsync(vacancyReview.VacancyReference);
            return new VacancyReviewSearchResultViewModel
            {
                IsAssignedToLoggedInUser = vacancyUser.UserId == vacancyReview.ReviewedByUser?.UserId,
                AssignedTo = vacancyReview.ReviewedByUser?.Name,
                AssignedTimeElapsed = vacancyReview.ReviewedDate.GetShortTimeElapsed(_timeProvider.Now),
                ClosingDate = vacancyReview.VacancySnapshot.ClosingDate.GetValueOrDefault(),
                EmployerName = vacancyReview.VacancySnapshot.EmployerName,
                VacancyReference = vacancyReview.VacancyReference.ToString(),
                VacancyTitle = vacancyReview.Title,
                ReviewId = vacancyReview.Id,
                IsClosed =  vacancyReview.Status == ReviewStatus.Closed,
                SubmittedDate = vacancyReview.VacancySnapshot.SubmittedDate.GetValueOrDefault(),
                IsAvailableForReview = isAvailableForReview,
                IsVacancyDeleted = vacancy.IsDeleted
            };
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
                TotalVacanciesResubmitted = dashboard.TotalVacanciesResubmitted
            };

            return vm;
        }
    }
}