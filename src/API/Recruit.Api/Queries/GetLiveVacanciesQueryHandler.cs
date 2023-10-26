using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetLiveVacanciesQueryHandler : IRequestHandler<GetLiveVacanciesQuery, GetLiveVacanciesQueryResponse>
{
    public async Task<GetLiveVacanciesQueryResponse> Handle(GetLiveVacanciesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
