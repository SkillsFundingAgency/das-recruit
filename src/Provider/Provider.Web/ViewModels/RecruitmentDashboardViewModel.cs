using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class RecruitmentDashboardViewModel
    {
        public IList<VacancySummary> Vacancies { get; set; }
        //public string WarningMessage { get; internal set; }
        //public string InfoMessage { get; internal set; }              
        //public string ResultsHeading { get; internal set; }
        public bool HasVacancies {get; internal set;}
        //public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
        //public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);         
        public FilteringOptions Filter { get; set; }       
        public bool HasOneVacancy => Vacancies.Count > 0;
        public string LinkUrl { get; set; }
        public int VacancyCountDraft => Vacancies.Count(v => v.Status == VacancyStatus.Draft);
        public int VacancyCountLive => Vacancies.Count(v => v.Status == VacancyStatus.Live);
        public int VacancyCountClosed => Vacancies.Count(v => v.Status == VacancyStatus.Closed);
        public int VacancyCountReferred => Vacancies.Count(v => v.Status == VacancyStatus.Referred);
        public int VacancyCountSubmitted => Vacancies.Count(v => v.Status == VacancyStatus.Submitted);
    }
}
