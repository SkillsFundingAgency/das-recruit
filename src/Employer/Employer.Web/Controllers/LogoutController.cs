using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.Services)]
    public class LogoutController : Controller
    {
        [HttpGet, Route("logout", Name = RouteNames.Logout_Get)]
        public async Task Logout()
        {
            await HttpContext.SignOutEmployerWebAsync();
        }
    }
}