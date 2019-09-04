using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEoiAgreementCookieWriter
    {
        string GetCookieFromRequest(HttpContext context);
        void WriteCookie(HttpResponse response, string userId, string employerAccountId, bool hasEoi);
        void DeleteCookie(HttpResponse response);
    }
}
