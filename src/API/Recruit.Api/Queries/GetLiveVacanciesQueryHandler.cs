using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
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
        var liveVacanciesToFetch = request.PageSize * request.PageNumber;

        var queryResult = liveVacanciesToFetch > 1000 ? await _queryStoreReader.GetAllLiveVacancies(1000) : await _queryStoreReader.GetAllLiveVacancies(liveVacanciesToFetch);

        if (queryResult == null) { return new GetLiveVacanciesQueryResponse { ResultCode = ResponseCode.NotFound }; }

        var totalLiveVacancies = queryResult.Count();
        var pageNo = PagingHelper.GetRequestedPageNo(request.PageNumber, request.PageSize, totalLiveVacancies);
        var totalPages = PagingHelper.GetTotalNoOfPages(request.PageSize, totalLiveVacancies);

        return new GetLiveVacanciesQueryResponse
        {
            ResultCode = ResponseCode.Success,
            Data = new LiveVacanciesSummary(queryResult, request.PageSize, pageNo, totalLiveVacancies, totalPages)
        };
    }
}
