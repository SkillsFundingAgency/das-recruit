using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task SignOutEmployerWebAsync(this HttpContext httpContext)
        {
            await httpContext.SignOutAsync("Cookies");

            await httpContext.SignOutAsync("oidc");
        }
    }
}
