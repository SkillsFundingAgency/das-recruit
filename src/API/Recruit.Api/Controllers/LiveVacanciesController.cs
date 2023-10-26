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
    public async Task<IActionResult> Get()
    {
        var resp = await _mediator.Send(new GetLiveVacanciesQuery());
        return GetApiResponse(resp);
    }
}
