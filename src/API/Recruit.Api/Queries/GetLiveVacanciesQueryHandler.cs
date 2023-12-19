using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using MediatR;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.VacancyServices.Wage;
using WageType = SFA.DAS.Recruit.Api.Models.WageType;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetLiveVacanciesQueryHandler : IRequestHandler<GetLiveVacanciesQuery, GetLiveVacanciesQueryResponse>
{
    private readonly IQueryStoreReader _queryStoreReader;

    public GetLiveVacanciesQueryHandler(IQueryStoreReader queryStoreReader)
    {
        _queryStoreReader = queryStoreReader;
    }

    public async Task<GetLiveVacanciesQueryResponse> Handle(GetLiveVacanciesQuery request, CancellationToken cancellationToken)
    {
        var vacanciesToGetCount = request.PageSize > 1000 ? 1000 : request.PageSize;
        var vacanciesToSkipCount = request.PageNumber < 2 ? 0 : (request.PageNumber - 1) * vacanciesToGetCount;
        

        var queryResult = await _queryStoreReader.GetAllLiveVacancies(vacanciesToSkipCount, vacanciesToGetCount);

        if (queryResult == null)
        {
            return new GetLiveVacanciesQueryResponse { ResultCode = ResponseCode.Success, Data = Enumerable.Empty<LiveVacancy>() };
        }

        foreach (var vacancy in queryResult)
        {
            if (vacancy.Wage.WageType == WageType.FixedWage.ToString())
            {
                vacancy.Wage.ApprenticeMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Under18NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Between18AndUnder21NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Between21AndUnder25NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
                vacancy.Wage.Over25NationalMinimumWage = vacancy.Wage.FixedWageYearlyAmount;
            }
            else if (vacancy.Wage.WageType == WageType.NationalMinimumWageForApprentices.ToString())
            {
                var weeklyHours = (int)decimal.Round(vacancy.Wage.WeeklyHours, MidpointRounding.AwayFromZero);
                var rates = NationalMinimumWageService.GetAnnualRates(vacancy.StartDate, weeklyHours);

                vacancy.Wage.ApprenticeMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Under18NationalMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Between18AndUnder21NationalMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Between21AndUnder25NationalMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Over25NationalMinimumWage = rates.ApprenticeMinimumWage;
            }
            else
            {
                var weeklyHours = (int) decimal.Round(vacancy.Wage.WeeklyHours, MidpointRounding.AwayFromZero);
                var rates = NationalMinimumWageService.GetAnnualRates(vacancy.StartDate, weeklyHours);

                vacancy.Wage.ApprenticeMinimumWage = rates.ApprenticeMinimumWage;
                vacancy.Wage.Under18NationalMinimumWage = rates.Under18NationalMinimumWage;
                vacancy.Wage.Between18AndUnder21NationalMinimumWage = rates.Between18AndUnder21NationalMinimumWage;
                vacancy.Wage.Between21AndUnder25NationalMinimumWage = rates.Between21AndUnder25NationalMinimumWage;
                vacancy.Wage.Over25NationalMinimumWage = rates.Over25NationalMinimumWage;
            }
        }

        var totalLiveVacanciesReturned = queryResult.Count();
        var liveVacanciesCount = await _queryStoreReader.GetAllLiveVacanciesCount();
        var pageNo = PagingHelper.GetRequestedPageNo(request.PageNumber, request.PageSize, (int)liveVacanciesCount);
        var totalPages = PagingHelper.GetTotalNoOfPages(request.PageSize, (int)liveVacanciesCount);

        return new GetLiveVacanciesQueryResponse
        {
            ResultCode = ResponseCode.Success,
            Data = new LiveVacanciesSummary(queryResult, request.PageSize, pageNo, totalLiveVacanciesReturned, (int)liveVacanciesCount, totalPages)
        };
    }
}
