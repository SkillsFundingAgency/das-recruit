using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using MediatR;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetTotalPositionsAvailableQueryHandler : IRequestHandler<GetTotalPositionsAvailableQuery, GetTotalPositionsAvailableQueryResult>
{
    private readonly IQueryStoreReader _queryStoreReader;

    public GetTotalPositionsAvailableQueryHandler(IQueryStoreReader queryStoreReader)
    {
        _queryStoreReader = queryStoreReader;
    }

    public async Task<GetTotalPositionsAvailableQueryResult> Handle(GetTotalPositionsAvailableQuery request, CancellationToken cancellationToken)
    {
        var count = await _queryStoreReader.GetTotalPositionsAvailableCount();
            
        return new GetTotalPositionsAvailableQueryResult
        {
            Data = count,
            ResultCode = ResponseCode.Success
        };
    }
}