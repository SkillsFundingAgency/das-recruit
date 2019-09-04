using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Services;

namespace Esfa.Recruit.Employer.Web.Controllers
{
    [Route(RoutePaths.Services)]
    public class LogoutController : Controller
    {
        private readonly ILevyDeclarationCookieWriter _levyDeclarationCookieWriter;
        private readonly IEoiAgreementCookieWriter _eoiAgreementCookieWriter;
        private readonly IEmployerAccountTypeCookieWriter _employerAccountTypeCookieWriter;

        public LogoutController(
            ILevyDeclarationCookieWriter levyDeclarationCookieWriter,
            IEoiAgreementCookieWriter eoiAgreementCookieWriter,
            IEmployerAccountTypeCookieWriter employerAccountTypeCookieWriter)
        {
            _levyDeclarationCookieWriter = levyDeclarationCookieWriter;
            _eoiAgreementCookieWriter = eoiAgreementCookieWriter;
            _employerAccountTypeCookieWriter = employerAccountTypeCookieWriter;
        }

        [HttpGet, Route("logout", Name = RouteNames.Logout_Get)]
        public async Task Logout()
        {
            _levyDeclarationCookieWriter.DeleteCookie(Response);
            _eoiAgreementCookieWriter.DeleteCookie(Response);
            _employerAccountTypeCookieWriter.DeleteCookie(Response);

            await HttpContext.SignOutEmployerWebAsync();
        }
    }
}