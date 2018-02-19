using Microsoft.AspNetCore.Http;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class RequestExtensions
    {
        public static string GetRequestUrl(this HttpRequest request)
        {
            var url = $"{request.Scheme}://{request.Host}";
            return url;
        }
    }
}
