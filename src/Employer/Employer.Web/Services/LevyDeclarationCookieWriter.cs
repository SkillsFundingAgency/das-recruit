using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class LevyDeclarationCookieWriter
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDataProtector _dataProtector;

        public LevyDeclarationCookieWriter(IHostingEnvironment hostingEnvironment, IDataProtectionProvider dataProtectionProvider)
        {
            _hostingEnvironment = hostingEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposes.LevyDeclarationCookie);
        }

        public string GetCookieFromRequest(HttpRequest request)
        {
            var content = request.Cookies[CookieNames.LevyEmployerIndicator];

            if (string.IsNullOrWhiteSpace(content))
                return null;

            return _dataProtector.Unprotect(content);
        }

        public void WriteCookie(HttpResponse response, string userId, string employerAccountId)
        {
            var protectedValue = _dataProtector.Protect($"{userId}-{employerAccountId}");

            response.Cookies.Append(CookieNames.LevyEmployerIndicator, protectedValue, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
        }

        public void DeleteCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieNames.LevyEmployerIndicator);
        }
    }
}