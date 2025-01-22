using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult GetApiResponse(ResponseBase resp)
    {
        return resp.ResultCode switch
        {
            ResponseCode.InvalidRequest => BadRequest(new { Errors = resp.ValidationErrors }),
            ResponseCode.NotFound => NotFound(),
            ResponseCode.Created => Created("", resp.Data),
            _ => Ok(resp.Data)
        };
    }
}