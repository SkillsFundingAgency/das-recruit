using System.Security.Cryptography;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class LevyDeclarationCookieWriter : ILevyDeclarationCookieWriter
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<LevyDeclarationCookieWriter> _logger;

        public LevyDeclarationCookieWriter(IHostingEnvironment hostingEnvironment, IDataProtectionProvider dataProtectionProvider, ILogger<LevyDeclarationCookieWriter> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposes.LevyDeclarationCookie);
            _logger = logger;
        }

        public string GetCookieFromRequest(HttpContext context)
        {
            string content = null;
            var encrtyptedContent = context.Request.Cookies[CookieNames.LevyEmployerIndicator];

            if (string.IsNullOrWhiteSpace(encrtyptedContent))
                return null;

            try
            {
                content = _dataProtector.Unprotect(encrtyptedContent);
            }
            catch (CryptographicException ex)
            {
                _logger.LogWarning(ex, "Couldn't decrypt levy cookie. Probably due to recent release changing DataProtection keys");
                DeleteCookie(context.Response);
            }

            return content;
        }

        public void WriteCookie(HttpResponse response, string userId, string employerAccountId, bool hasLevyDeclaration)
        {
            var protectedValue = _dataProtector.Protect(userId + "/" + employerAccountId + "/" + hasLevyDeclaration);

            response.Cookies.Append(CookieNames.LevyEmployerIndicator, protectedValue, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
        }

        public void DeleteCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieNames.LevyEmployerIndicator);
        }
    }
}