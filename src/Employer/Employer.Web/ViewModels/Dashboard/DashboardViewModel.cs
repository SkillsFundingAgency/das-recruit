using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Humanizer;

namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public string EmployerAccountId { get; set; }
        public IList<VacancySummary> Vacancies { get; set; }
        public AlertsViewModel Alerts { get; internal set; }

        public bool HasOneVacancy => Vacancies.Count == 1;
        public Guid CurrentVacancyId => HasOneVacancy ? Vacancies.Single().Id : new Guid();
        public int VacancyCountDraft => Vacancies.Count(v => v.Status == VacancyStatus.Draft);
        public string VacancyTextDraft => "advert".ToQuantity(VacancyCountDraft, ShowQuantityAs.None);
        public bool HasDraftVacancy => VacancyCountDraft > 0;
        public int VacancyCountLive => Vacancies.Count(v => v.Status == VacancyStatus.Live);
        public string VacancyTextLive => "advert".ToQuantity(VacancyCountLive, ShowQuantityAs.None);
        public bool HasLiveVacancy => VacancyCountLive > 0;
        public int VacancyCountClosed => Vacancies.Count(v => v.Status == VacancyStatus.Closed);
        public string VacancyTextClosed => "advert".ToQuantity(VacancyCountClosed, ShowQuantityAs.None);
        public bool HasClosedVacancy => VacancyCountClosed > 0;
        public int VacancyCountReferred => Vacancies.Count(v => v.Status == VacancyStatus.Referred);
        public string VacancyTextReferred => "advert".ToQuantity(VacancyCountReferred, ShowQuantityAs.None);
        public bool HasReferredVacancy => VacancyCountReferred > 0;
        public int VacancyCountSubmitted => Vacancies.Count(v => v.Status == VacancyStatus.Submitted);
        public bool HasSubmittedVacancy => VacancyCountSubmitted > 0;
        public int NoOfNewApplications => Vacancies.Count(v => v.NoOfNewApplications > 0);
        public bool HasNewApplications => NoOfNewApplications > 0;
        public int AllApplications => Vacancies.Count(v => v.NoOfApplications > 0);
        public bool ShowAllApplications => AllApplications > 0;
        public string ApplicationTextLive => "application".ToQuantity(NoOfNewApplications, ShowQuantityAs.None);
        public int NoOfVacanciesClosingSoon { get; set; }
        public string VacancyTextClosingSoon => "advert".ToQuantity(NoOfVacanciesClosingSoon, ShowQuantityAs.None);
        public int NoOfVacanciesClosingSoonWithNoApplications { get; set; }
        public string VacancyTextClosingSoonWithNoApplications => "advert".ToQuantity(NoOfVacanciesClosingSoonWithNoApplications, ShowQuantityAs.None);
        public bool ShowNoOfVacanciesClosingSoon => NoOfVacanciesClosingSoon > 0;
        public bool ShowNoOfVacanciesClosingSoonWithNoApplications => NoOfVacanciesClosingSoonWithNoApplications > 0;
        public bool HasAnyVacancies => Vacancies.Any();
        public bool FromMaHome { get; set; }
    }
}