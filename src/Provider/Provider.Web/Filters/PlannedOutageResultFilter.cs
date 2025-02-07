using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Provider.Web.Filters
{
    public class PlannedOutageResultFilter : ResultFilterAttribute
    {
        private readonly ProviderRecruitSystemConfiguration _systemConfig;

        public PlannedOutageResultFilter(ProviderRecruitSystemConfiguration systemConfig)
        {
            _systemConfig = systemConfig;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.HttpContext.Request.Cookies.ContainsKey(CookieNames.SeenOutageMessage) == false)
            {
                if (_systemConfig != null && string.IsNullOrEmpty(_systemConfig.PlannedOutageMessage) == false)
                {
                    var ctrlr = (Controller)context.Controller;
                    ctrlr.ViewData.Add(ViewDataKeys.CanShowOutageMessage, true);
                    ctrlr.ViewData.Add(ViewDataKeys.PlannedOutageMessage, _systemConfig.PlannedOutageMessage);
                }
            }

            await next();
        }
    }
}