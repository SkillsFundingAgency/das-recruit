using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers;

[Route("api/vacancies/{vacancyReference:long}/[controller]")]
public class ApplicantsController(IMediator mediator) : ApiControllerBase
{
    // GET api/vacancies/{vacancyReference}/applicants?outcome
    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] ulong vacancyReference, [FromQuery] string outcome)
    {
        var resp = await mediator.Send(new GetApplicantsQuery((long)vacancyReference, outcome?.Trim()));
        return GetApiResponse(resp);
    }
}