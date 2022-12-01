using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA
{
    public class QaDashboard : QueryProjectionBase
    {
        public QaDashboard() : base(QueryViewType.QaDashboard.TypeName)
        {
        }

        public int TotalVacanciesForReview { get; set; }
        public int TotalVacanciesBrokenSla { get; set; }
        public int TotalVacanciesResubmitted { get; set; }
        public List<VacancyReview> SearchResults { get; set; }
        public int TotalVacanciesSubmittedTwelveTwentyFourHours { get; set; }
    }
}