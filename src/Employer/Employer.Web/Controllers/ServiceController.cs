using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace Esfa.Recruit.Employer.Web.Controllers;

[AllowAnonymous]
[Route(RoutePaths.Services, Name = "Service", Order = 0)]
public class ServiceController(IConfiguration config, IStubAuthenticationService stubAuthenticationService)
    : Controller
{
#if DEBUG
    [HttpGet]
    [Route("SignIn-Stub")]
    [AllowAnonymous]
    public IActionResult SigninStub()
    {
        return View("SigninStub", new List<string>{config["StubId"],config["StubEmail"]});
    }
    [HttpPost]
    [Route("SignIn-Stub")]
    [AllowAnonymous]
    public async Task<IActionResult> SigninStubPost()
    {
        var claims = await stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
        {
            Email = config["StubEmail"],
            Id = config["StubId"]
        });

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
            new AuthenticationProperties());

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