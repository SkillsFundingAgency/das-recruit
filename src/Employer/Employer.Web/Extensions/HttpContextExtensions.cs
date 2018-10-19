using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task SignOutEmployerWebAsync(this HttpContext httpContext, bool signOutOidc = true)
        {
            await httpContext.SignOutAsync("Cookies");
            await httpContext.SignOutAsync("oidc");
        }
    }
}
