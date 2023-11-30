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
    public async Task<IActionResult> Get([FromQuery] uint pageSize = 100, [FromQuery] uint pageNo = 1)
    {
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
}
