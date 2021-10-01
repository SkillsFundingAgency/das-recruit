using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Models
{
    public class VacanciesSummary
    {
        public IEnumerable<VacancySummary> Vacancies { get; }
        public int PageSize { get; }
        public int PageNo { get; }
        public int TotalResults { get; }
        public int TotalPages { get; }

        public VacanciesSummary(IEnumerable<VacancySummary> vacancies, int pageSize, int pageNo, int totalResults, int totalPages)
        {
            Vacancies = vacancies;
            PageSize = pageSize;
            PageNo = pageNo;
            TotalResults = totalResults;
            TotalPages = totalPages;
        }
    }
}