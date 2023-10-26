using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace SFA.DAS.Recruit.Api.Models;

public class LiveVacanciesSummary
{
    public LiveVacanciesSummary(IEnumerable<LiveVacancy> vacancies, int pageSize, int pageNo, int totalResults, int totalPages)
    {
        Vacancies = vacancies;
        PageSize = pageSize;
        PageNo = pageNo;
        TotalResults = totalResults;
        TotalPages = totalPages;
    }

    public IEnumerable<LiveVacancy> Vacancies { get; }
    public int PageSize { get; }
    public int PageNo { get; }
    public int TotalResults { get; }
    public int TotalPages { get; }

}
