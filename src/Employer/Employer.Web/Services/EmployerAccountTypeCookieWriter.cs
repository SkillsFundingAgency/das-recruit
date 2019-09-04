using System.Security.Cryptography;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class EmployerAccountTypeCookieWriter : IEmployerAccountTypeCookieWriter
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<EmployerAccountTypeCookieWriter> _logger;

        public EmployerAccountTypeCookieWriter(
            IHostingEnvironment hostingEnvironment,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<EmployerAccountTypeCookieWriter> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurposes.EmployerAccountTypeCookie);
            _logger = logger;
        }

        public string GetCookieFromRequest(HttpContext context)
        {
            string content = null;
            var encrtyptedContent = context.Request.Cookies[CookieNames.EmployerAccountType];

            if (string.IsNullOrWhiteSpace(encrtyptedContent))
                return null;

            try
            {
                content = _dataProtector.Unprotect(encrtyptedContent);
            }
            catch (CryptographicException ex)
            {
                _logger.LogWarning(ex, "Couldn't decrypt employer account type cookie. Probably due to recent release changing DataProtection keys");
                DeleteCookie(context.Response);
            }

            return content;
        }

        public void WriteCookie(HttpResponse response, string userId, string employerAccountId, string employerAccountType)
        {
            var protectedValue = _dataProtector.Protect(userId + '/' + employerAccountId + '/' + employerAccountType);

            response.Cookies.Append(CookieNames.EmployerAccountType, protectedValue, EsfaCookieOptions.GetDefaultHttpCookieOption(_hostingEnvironment));
        }

        public void DeleteCookie(HttpResponse response)
        {
            response.Cookies.Delete(CookieNames.EmployerAccountType);
        }
    }
}
