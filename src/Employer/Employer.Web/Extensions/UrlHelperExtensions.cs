using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string ExternalUrlAction(this IUrlHelper helper, string baseUrl, string controllerName, string actionName = "", bool ignoreAccountId = false)
        {
            var accountId = helper.ActionContext.RouteData.Values["employerAccountId"];

            baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";

            return ignoreAccountId ? $"{baseUrl}{controllerName}/{actionName}"
                  : $"{baseUrl}accounts/{accountId}/{controllerName}/{actionName}";
        }
    }
}