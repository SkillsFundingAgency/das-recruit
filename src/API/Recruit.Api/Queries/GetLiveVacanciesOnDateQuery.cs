using System;
using MediatR;

namespace SFA.DAS.Recruit.Api.Queries;

public class GetLiveVacanciesOnDateQuery : IRequest<GetLiveVacanciesOnDateQueryResult>
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public DateTime ClosingDate { get; set; }
}