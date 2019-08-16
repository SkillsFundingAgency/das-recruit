using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEmployerAccountTypeCookieWriter
    {
        string GetCookieFromRequest(HttpContext context);
        void WriteCookie(HttpResponse response, string userId, string employerAccountId, string employerAccountType);
        void DeleteCookie(HttpResponse response);
    }
}
