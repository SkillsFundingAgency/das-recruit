using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Models;

namespace SFA.DAS.Recruit.Api.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult GetApiResponse(ResponseBase resp)
        {
            switch (resp.ResultCode)
            {
                case ResponseCode.InvalidRequest:
                    return BadRequest(new {Errors = resp.ValidationErrors});
                case ResponseCode.NotFound:
                    return NotFound();
                case ResponseCode.Created:
                    return Created("", resp.Data);
                default:
                    return Ok(resp.Data);
            }
        }
    }
}