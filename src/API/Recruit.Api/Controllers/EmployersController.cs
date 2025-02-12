using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers;

[Route("api/[controller]")]
public class EmployersController(IMediator mediator) : ApiControllerBase
{
    // GET api/employers/?employerAccountId
    [HttpGet("{employerAccountId:minlength(6)}")]
    public async Task<IActionResult> Get([FromRoute] string employerAccountId)
    {
        var resp = await mediator.Send(new GetEmployerSummaryQuery(employerAccountId.Trim().ToUpper()));
        return GetApiResponse(resp);
    }

    [HttpGet("{employerAccountId:minlength(6)}/successful-applicants")]
    public async Task<IActionResult> GetSuccessfulApplicants([FromRoute] string employerAccountId)
    {
        var response = await mediator.Send(new GetEmployerSuccessfulApplicantsQuery(employerAccountId.Trim().ToUpper()));
        return GetApiResponse(response);
    }
}