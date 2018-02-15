using Microsoft.AspNetCore.Mvc.Routing;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string ExternalUrlAction(this UrlHelper helper, string controllerName, string actionName = "", bool ignoreAccountId = false)
        {            
            var baseUrl = GetMyaBaseUrl();
            
            //var accountId = helper.RequestContext.RouteData.Values["hashedAccountId"];
            var accountId = helper.ActionContext.RouteData.Values["employerAccountId"];

            
            return ignoreAccountId ? $"{baseUrl}{controllerName}/{actionName}"
                                    : $"{baseUrl}accounts/{accountId}/{controllerName}/{actionName}";
        }

        private static string GetMyaBaseUrl()
        {
            return "https://at-eas.apprenticeships.sfa.bis.gov.uk/";
        }

        private static string GetBaseUrl()
        {
            return string.Empty;
            //return CloudConfigurationManager.GetSetting("MyaBaseUrl").EndsWith("/")
            //    ? CloudConfigurationManager.GetSetting("MyaBaseUrl")
            //    : CloudConfigurationManager.GetSetting("MyaBaseUrl") + "/";
        }
    }
}
