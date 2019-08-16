using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface ILevyDeclarationCookieWriter
    {
        string GetCookieFromRequest(HttpContext context);
        void WriteCookie(HttpResponse response, string userId, string employerAccountId, bool hasLevyDeclaration);
        void DeleteCookie(HttpResponse response);
    }
}
