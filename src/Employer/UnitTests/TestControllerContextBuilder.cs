using System.Collections.Generic;
using System.Security.Claims;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Esfa.Recruit.Employer.UnitTests;

public class TestControllerContextBuilder(Controller controller)
{
    public TestUserClaimBuilder WithUser(Guid userId)
    {
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        var builder = new TestUserClaimBuilder(controller.ControllerContext.HttpContext.User);
        builder.WithClaim(EmployerRecruitClaims.IdamsUserIdClaimTypeIdentifier, userId.ToString());
        return builder;
    }

    public TestControllerContextBuilder WithTempData()
    {
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        return this;
    }
}

public class TestUserClaimBuilder(ClaimsPrincipal claimsPrincipal)
{
    public TestUserClaimBuilder WithClaim(string type, string value)
    {
        if (claimsPrincipal.Identity is null)
        {
            claimsPrincipal.AddIdentity(
                new ClaimsIdentity(new List<Claim>
                {
                    new (type, value)
                }));
        }
        else
        {
            (claimsPrincipal.Identity as ClaimsIdentity)?.AddClaim(new Claim(type, value));
        }

        return this;
    }
}

public static class ControllerExtensions
{
    public static TestControllerContextBuilder AddControllerContext(this Controller controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        
        return new TestControllerContextBuilder(controller);
    }
}