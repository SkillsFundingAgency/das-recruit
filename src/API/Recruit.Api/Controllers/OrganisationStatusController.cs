using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers;

[Route("api")]
public class OrganisationStatusController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OrganisationStatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/providers/11111111/status
    [HttpGet("providers/{ukprn:long:min(10000000)}/status")]
    public async Task<IActionResult> Get(long ukprn)
    {
        var resp = await _mediator.Send(new GetProviderOrganisationStatusQuery(ukprn));
        return GetApiResponse(resp);
    }

    // GET api/employers/ABC123/status
    [HttpGet("employers/{employerAccountId:minlength(6)}/status")]
    public async Task<IActionResult> Get(string employerAccountId)
    {
        var resp = await _mediator.Send(new GetEmployerOrganisationStatusQuery(employerAccountId.Trim().ToUpper()));
        return GetApiResponse(resp);
    }
}