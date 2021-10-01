using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers
{
    [Route("api/[controller]")]
    public class ReferenceDataController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public ReferenceDataController (IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("candidate-skills")]
        public async Task<IActionResult> GetCandidateSkills()
        {
            var result = await _mediator.Send(new GetSkillsQuery());

            return GetApiResponse(result);
        }

        [HttpGet]
        [Route("candidate-qualifications")]
        public async Task<IActionResult> GetCandidateQualifications()
        {
            var result = await _mediator.Send(new GetQualificationsQuery());

            return GetApiResponse(result);
        }

    }
}