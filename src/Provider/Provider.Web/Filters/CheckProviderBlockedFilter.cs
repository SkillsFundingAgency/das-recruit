using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Controllers;
using Esfa.Recruit.Provider.Web.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Provider.Web.Filters
{
    public class CheckProviderBlockedFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;        

        public CheckProviderBlockedFilter(IConfiguration configuration)
        {
            _configuration = configuration;            
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (RequestIsForWhiteListedPage(context) == false)
            {
                var blockedProviderAccountIds = _configuration.GetValue<string>("WhitelistedProvidersList").Split(';').ToList();

                var ukprnFromUrl = context.RouteData.Values[RouteValues.Ukprn].ToString().ToUpper();

                if (!blockedProviderAccountIds.Contains(ukprnFromUrl))
                {
                    throw new BlockedProviderException($"Provider Ukprn account '{ukprnFromUrl}' is blocked");
                }
            }
            
            await next();
        }

        private bool RequestIsForWhiteListedPage(ActionExecutingContext context)
        {
            var controllerName = (((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ControllerTypeInfo).Name;

            var whitelistControllers = new List<string> { nameof(ErrorController), nameof(LogoutController), nameof(ExternalLinksController), nameof(ContentPolicyReportController) };

            return whitelistControllers.Contains(controllerName);
        }        
    }
}
