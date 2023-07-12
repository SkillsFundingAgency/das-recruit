using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.Alerts;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public EmployerDashboardSummary EmployerDashboardSummary { get; set; }
        public string EmployerAccountId { get; set; }
        public AlertsViewModel Alerts { get; internal set; }

        public int VacancyCountDraft => EmployerDashboardSummary.Draft;
        public string VacancyTextDraft => "advert".ToQuantity(VacancyCountDraft, ShowQuantityAs.None);
        public bool HasDraftVacancy => VacancyCountDraft > 0;
        public int VacancyCountReview => EmployerDashboardSummary.Review;
        public string VacancyTextReview => "advert".ToQuantity(VacancyCountReview, ShowQuantityAs.None);
        public int VacancyCountLive => EmployerDashboardSummary.Live;
        public string VacancyTextLive => "advert".ToQuantity(VacancyCountLive, ShowQuantityAs.None);
        public int VacancyCountClosed => EmployerDashboardSummary.Closed;
        public string VacancyTextClosed => "advert".ToQuantity(VacancyCountClosed, ShowQuantityAs.None);
        public int VacancyCountReferred => EmployerDashboardSummary.Referred;
        public string VacancyTextReferred => "advert".ToQuantity(VacancyCountReferred, ShowQuantityAs.None);
        public int VacancyCountSubmitted => EmployerDashboardSummary.Submitted;
        public int NoOfNewApplications => EmployerDashboardSummary.NumberOfNewApplications;
        public bool HasNewApplications => NoOfNewApplications > 0;
        public int NoOfSharedApplications => EmployerDashboardSummary.NumberOfSharedApplications;
        public bool HasSharedApplications => NoOfSharedApplications > 0;
        public string SharedApplicationsText => "application".ToQuantity(NoOfSharedApplications, ShowQuantityAs.None);
        public bool ShowAllSharedApplications => HasSharedApplications;
        public bool ShowAllApplications => EmployerDashboardSummary.HasApplications;
        public string ApplicationTextLive => "application".ToQuantity(NoOfNewApplications, ShowQuantityAs.None);
        public int NoOfVacanciesClosingSoon => EmployerDashboardSummary.NumberClosingSoon;
        public string VacancyTextClosingSoon => "advert".ToQuantity(NoOfVacanciesClosingSoon, ShowQuantityAs.None);

        public int NoOfVacanciesClosingSoonWithNoApplications => EmployerDashboardSummary.NumberClosingSoonWithNoApplications;
        public string VacancyTextClosingSoonWithNoApplications => "advert".ToQuantity(NoOfVacanciesClosingSoonWithNoApplications, ShowQuantityAs.None);
        public bool ShowNoOfVacanciesClosingSoon => NoOfVacanciesClosingSoon > 0;
        public bool ShowNoOfVacanciesClosingSoonWithNoApplications => NoOfVacanciesClosingSoonWithNoApplications > 0;
        public bool HasAnyVacancies => EmployerDashboardSummary.HasVacancies;
        public int NumberOfVacancies => EmployerDashboardSummary.NumberOfVacancies;
        public bool FromMaHome { get; set; }
        public bool HasEmployerReviewPermission { get; set; }
        
    }
}