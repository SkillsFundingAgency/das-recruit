using System.Threading.Tasks;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Employer.Web.Filters
{
    public class PlannedOutageResultFilter : ResultFilterAttribute
    {
        private readonly EmployerRecruitSystemConfiguration _systemConfig;

        public PlannedOutageResultFilter(EmployerRecruitSystemConfiguration systemConfig)
        {
            _systemConfig = systemConfig;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.HttpContext.Request.Cookies.ContainsKey(CookieNames.SeenOutageMessage) == false)
            {
                if (string.IsNullOrEmpty(_systemConfig.PlannedOutageMessage) == false)
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