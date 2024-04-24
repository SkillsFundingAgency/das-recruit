using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers
{
    [Route("api/[controller]")]
    public class ClosedVacanciesController(IMediator mediator) : ApiControllerBase
    {
        [HttpGet]
        [Route("{vacancyReference}")]
        public async Task<IActionResult> Get([FromRoute] long vacancyReference)
        {
            var resp = await mediator.Send(new GetClosedVacancyQuery(vacancyReference));
            return GetApiResponse(resp);
        }
    }
}
