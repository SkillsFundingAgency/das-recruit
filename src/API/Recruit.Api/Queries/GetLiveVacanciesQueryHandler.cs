using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;

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
        var queryResult = await _queryStoreReader.GetAllLiveVacancies();

        if (queryResult == null) { return new GetLiveVacanciesQueryResponse { ResultCode = Models.ResponseCode.NotFound }; }

        //var numberResults = queryResult.ToList().Count();

        return new GetLiveVacanciesQueryResponse { ResultCode = Models.ResponseCode.Success, Data = queryResult.ToList() };

        // implement paging
    }
}
