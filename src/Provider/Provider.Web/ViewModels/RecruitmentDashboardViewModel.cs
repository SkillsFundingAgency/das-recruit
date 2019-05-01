using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using Humanizer;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class RecruitmentDashboardViewModel
    {
        public IList<VacancySummary> Vacancies { get; set; }
        public bool HasVacancies {get; internal set;}           
        public FilteringOptions Filter { get; set; }
        public bool HasOneVacancy => Vacancies.Count > 0;
        public string LinkUrl { get; set; }
        public int VacancyCountDraft => Vacancies.Count(v => v.Status == VacancyStatus.Draft);        
        public string VacancyTextDraft => "vacancy".ToQuantity(VacancyCountDraft, ShowQuantityAs.None);
        public bool HasDraftVacancy => VacancyCountDraft > 0;
        public int VacancyCountLive => Vacancies.Count(v => v.Status == VacancyStatus.Live);
        public string VacancyTextLive => "vacancy".ToQuantity(VacancyCountLive, ShowQuantityAs.None);
        public bool HasLiveVacancy => VacancyCountLive > 0;
        public int VacancyCountClosed => Vacancies.Count(v => v.Status == VacancyStatus.Closed);
        public string VacancyTextClosed => "vacancy".ToQuantity(VacancyCountClosed, ShowQuantityAs.None);
        public bool HasClosedVacancy => VacancyCountClosed > 0;
        public int VacancyCountReferred => Vacancies.Count(v => v.Status == VacancyStatus.Referred);
        public string VacancyTextReferred => "vacancy".ToQuantity(VacancyCountReferred, ShowQuantityAs.None);
        public bool HasReferredVacancy => VacancyCountReferred > 0;
        public int VacancyCountSubmitted => Vacancies.Count(v => v.Status == VacancyStatus.Submitted);
        public bool HasSubmittedVacancy => VacancyCountSubmitted > 0;
    }
}
