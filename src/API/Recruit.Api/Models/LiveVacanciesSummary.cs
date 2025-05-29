using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace SFA.DAS.Recruit.Api.Models;

public class LiveVacanciesSummary
{
    public LiveVacanciesSummary(IEnumerable<LiveVacancy> vacancies, int pageSize, int pageNo, int totalLiveVacanciesReturned, int totalLiveVacancies, int totalPages)
    {
        Vacancies = vacancies;
        PageSize = pageSize;
        PageNo = pageNo;
        TotalLiveVacanciesReturned = totalLiveVacanciesReturned;
        TotalLiveVacancies = totalLiveVacancies;
        TotalPages = totalPages;
    }

    public IEnumerable<LiveVacancy> Vacancies { get; }
    public int PageSize { get; }
    public int PageNo { get; }
    public int TotalLiveVacanciesReturned { get; set; }
    public int TotalLiveVacancies { get; }
    public int TotalPages { get; }
}
