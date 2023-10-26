using MediatR;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetLiveVacanciesQuery : IRequest<GetLiveVacanciesQueryResponse>
{
    public GetLiveVacanciesQuery(int pageSize, int pageNumber)
    {
        PageSize = pageSize;
        PageNumber = pageNumber;
    }

    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}