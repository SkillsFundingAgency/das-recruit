using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers
{
    [Route("api/[controller]")]
    public class VacanciesController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public VacanciesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/vacancies
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]string employerAccountId, uint? legalEntityId, ulong? ukprn, uint pageSize = 25, uint pageNo = 1)
        {
            var resp = await _mediator.Send(new GetVacanciesQuery(employerAccountId.Trim().ToUpper(), (int?)legalEntityId, (long?)ukprn, (int)pageSize, (int)pageNo));
            return GetApiResponse(resp);
        }
    }
}
