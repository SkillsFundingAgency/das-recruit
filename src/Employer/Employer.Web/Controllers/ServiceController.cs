using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web.Controllers;

[AllowAnonymous]
[Route(RoutePaths.Services, Name = "Service", Order = 0)]
public class ServiceController : Controller
{
    private readonly IConfiguration _config;
    private readonly IStubAuthenticationService _stubAuthenticationService;

    public ServiceController(IConfiguration config, IStubAuthenticationService stubAuthenticationService)
    {
        _config = config;
        _stubAuthenticationService = stubAuthenticationService;
    }
    
#if DEBUG
    [HttpGet]
    [Route("SignIn-Stub")]
    [AllowAnonymous]
    public IActionResult SigninStub()
    {
        return View("SigninStub", new List<string>{_config["StubId"],_config["StubEmail"]});
    }
    [HttpPost]
    [Route("SignIn-Stub")]
    [AllowAnonymous]
    public IActionResult SigninStubPost()
    {
        _stubAuthenticationService?.AddStubEmployerAuth(Response.Cookies, new StubAuthUserDetails
        {
            Email = _config["StubEmail"],
            Id = _config["StubId"]
        });

        return RedirectToRoute("Signed-in-stub");
    }

    [Authorize]
    [HttpGet]
    [Route("signed-in-stub", Name = "Signed-in-stub")]
    public IActionResult SignedInStub()
    {
        return View();
    }
#endif
}