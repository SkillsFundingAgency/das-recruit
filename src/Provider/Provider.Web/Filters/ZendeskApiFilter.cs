using System;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Filters;

public class ZendeskApiFilter(ILogger<ZendeskApiFilter> logger) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        try
        {
            var controller = filterContext.Controller as Controller;
            if (controller?.User != null)
            {
                var user = controller.User.ToVacancyUser();
                controller.ViewBag.ZendeskApiData = new ZendeskApiData
                {
                    Name = user.Name,
                    Email = user.Email,
                    Organization = user.Ukprn.HasValue ? user.Ukprn.Value.ToString() : string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"ZendeskApiFilter Cannot set ZendeskApiData for Provider");
        }
            
        base.OnActionExecuting(filterContext);
    }

    public class ZendeskApiData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
    }
}