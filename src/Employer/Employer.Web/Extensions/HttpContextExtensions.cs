using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task SignOutEmployerWebAsync(this HttpContext httpContext, string redirectUrl = null)
        {
            AuthenticationProperties properties = null;

            await httpContext.SignOutAsync("Cookies");

            if (redirectUrl != null)
            {
                properties = new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };
            }

            await httpContext.SignOutAsync("oidc", properties);
        }
    }
}
