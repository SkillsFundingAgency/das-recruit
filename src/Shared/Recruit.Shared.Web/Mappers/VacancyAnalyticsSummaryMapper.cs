using System;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics;

namespace Esfa.Recruit.Shared.Web.Mappers
{
    public class VacancyAnalyticsSummaryMapper
    {
        public static VacancyAnalyticsSummaryViewModel MapToVacancyAnalyticsSummaryViewModel(VacancyAnalyticsSummary vacancyAnalyticsSummary, DateTime liveDate)
        {
            var summaryViewModel = new VacancyAnalyticsSummaryViewModel
            {
                LiveDate = liveDate,
                LastUpdatedDate = vacancyAnalyticsSummary.LastUpdated == DateTime.MinValue ? DateTime.UtcNow : vacancyAnalyticsSummary.LastUpdated,

                NoOfTimesAppearedInSearch = vacancyAnalyticsSummary.NoOfApprenticeshipSearches,
                NoOfTimesAppearedInSearchOverLastSevenDays = vacancyAnalyticsSummary.NoOfApprenticeshipSearchesSevenDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesSixDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesFiveDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesFourDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesThreeDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesTwoDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipSearchesOneDayAgo,
                NoOfApplicationsStarted = vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreated,
                NoOfApplicationsStartedOverLastSevenDays = vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedSevenDaysAgo
                                                        + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedSixDaysAgo
                                                        + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedFiveDaysAgo
                                                        + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedFourDaysAgo
                                                        + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedThreeDaysAgo
                                                        + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedTwoDaysAgo
                                                        + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsCreatedOneDayAgo,
                NoOfApplicationsSubmitted = vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmitted,
                NoOfApplicationsSubmittedOverLastSevenDays = vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedSevenDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedSixDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedFiveDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedFourDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedThreeDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedTwoDaysAgo
                                                            + vacancyAnalyticsSummary.NoOfApprenticeshipApplicationsSubmittedOneDayAgo,
                NoOfTimesViewed = vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViews,
                NoOfTimesViewedOverLastSevenDays = vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsSevenDaysAgo
                                                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsSixDaysAgo
                                                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsFiveDaysAgo
                                                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsFourDaysAgo
                                                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsThreeDaysAgo
                                                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsTwoDaysAgo
                                                + vacancyAnalyticsSummary.NoOfApprenticeshipDetailsViewsOneDayAgo,
            };

            return summaryViewModel;
        }
    }
}