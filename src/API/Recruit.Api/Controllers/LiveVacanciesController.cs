using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace SFA.DAS.Recruit.Api.Controllers;

[Route("api/[controller]")]
public class LiveVacanciesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public LiveVacanciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] uint pageSize = 100, [FromQuery] uint pageNo = 1, [FromQuery] DateTime? closingDate = null)
    {
        if (closingDate != null)
        {
            var dateResp = await _mediator.Send(new GetLiveVacanciesOnDateQuery
            {
                PageSize = (int)pageSize,
                PageNumber = (int)pageNo,
                ClosingDate = closingDate.Value
            });
            return GetApiResponse(dateResp);    
        }
        
        var resp = await _mediator.Send(new GetLiveVacanciesQuery((int)pageSize, (int)pageNo));
        return GetApiResponse(resp);
    }
    
    [HttpGet]
    [Route("{vacancyReference}")]
    public async Task<IActionResult> Get([FromRoute] long vacancyReference)
    {
        var resp = await _mediator.Send(new GetLiveVacancyQuery(vacancyReference));
        return GetApiResponse(resp);
    }

    [HttpGet]
    [Route("total-positions-available")]
    public async Task<IActionResult> GetTotalPositionsAvailable()
    {
        var resp = await _mediator.Send(new GetTotalPositionsAvailableQuery());
        return GetApiResponse(resp);
    }
}
