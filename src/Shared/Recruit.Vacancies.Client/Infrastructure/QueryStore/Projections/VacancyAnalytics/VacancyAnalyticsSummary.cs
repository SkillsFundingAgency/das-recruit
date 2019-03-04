namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyAnalytics
{
    public class VacancyAnalyticsSummary : QueryProjectionBase
    {
        public VacancyAnalyticsSummary() : base(QueryViewType.VacancyAnalyticsSummary.TypeName)
        {
        }

        public long VacancyReference { get; set; }

        public int NoOfApprenticeshipSearches { get; set; }


        public int NoOfApprenticeshipSearchesSevenDaysAgo { get; set; }
        public int NoOfApprenticeshipSearchesSixDaysAgo { get; set; }
        public int NoOfApprenticeshipSearchesFiveDaysAgo { get; set; }
        public int NoOfApprenticeshipSearchesFourDaysAgo { get; set; }
        public int NoOfApprenticeshipSearchesThreeDaysAgo { get; set; }
        public int NoOfApprenticeshipSearchesTwoDaysAgo { get; set; }
        public int NoOfApprenticeshipSearchesOneDayAgo { get; set; }



        public int NoOfApprenticeshipSavedSearchAlerts { get; set; }

        public int NoOfApprenticeshipSavedSearchAlertsSevenDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSearchAlertsSixDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSearchAlertsFiveDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSearchAlertsFourDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSearchAlertsThreeDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSearchAlertsTwoDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSearchAlertsOneDayAgo { get; set; }




        public int NoOfApprenticeshipSaved { get; set; }

        public int NoOfApprenticeshipSavedSevenDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedSixDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedFiveDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedFourDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedThreeDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedTwoDaysAgo { get; set; }
        public int NoOfApprenticeshipSavedOneDayAgo { get; set; }




        public int NoOfApprenticeshipDetailsViews { get; set; }

        public int NoOfApprenticeshipDetailsViewsSevenDaysAgo { get; set; }
        public int NoOfApprenticeshipDetailsViewsSixDaysAgo { get; set; }
        public int NoOfApprenticeshipDetailsViewsFiveDaysAgo { get; set; }
        public int NoOfApprenticeshipDetailsViewsFourDaysAgo { get; set; }
        public int NoOfApprenticeshipDetailsViewsThreeDaysAgo { get; set; }
        public int NoOfApprenticeshipDetailsViewsTwoDaysAgo { get; set; }
        public int NoOfApprenticeshipDetailsViewsOneDayAgo { get; set; }



        public int NoOfApprenticeshipApplicationsCreated { get; set; }

        public int NoOfApprenticeshipApplicationsCreatedSevenDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsCreatedSixDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsCreatedFiveDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsCreatedFourDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsCreatedThreeDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsCreatedTwoDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsCreatedOneDayAgo { get; set; }





        public int NoOfApprenticeshipApplicationsSubmitted { get; set; }

        public int NoOfApprenticeshipApplicationsSubmittedSevenDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsSubmittedSixDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsSubmittedFiveDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsSubmittedFourDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsSubmittedThreeDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsSubmittedTwoDaysAgo { get; set; }
        public int NoOfApprenticeshipApplicationsSubmittedOneDayAgo { get; set; }
    }
}