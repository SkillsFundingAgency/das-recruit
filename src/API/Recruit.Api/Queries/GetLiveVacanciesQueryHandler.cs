using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;
using MediatR;
using SFA.DAS.Recruit.Api.Extensions;
using SFA.DAS.Recruit.Api.Helpers;
using SFA.DAS.Recruit.Api.Models;

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

        queryResult.ToList().ForEach(x => x.AddWageData());

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
