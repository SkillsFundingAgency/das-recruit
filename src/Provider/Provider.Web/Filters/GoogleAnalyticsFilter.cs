using System;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Provider.Shared.UI.Models;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Filters;

public class GoogleAnalyticsFilter(ILogger<GoogleAnalyticsFilter> logger) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        try
        {
            var controller = filterContext.Controller as Controller;
            if (controller?.User != null)
            {

                var user = controller.User.ToVacancyUser();
                controller.ViewBag.GaData = new GaData
                {
                    UserId = user.DfEUserId,
                    UkPrn = user.Ukprn.HasValue ? user.Ukprn.Value.ToString() : string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"GoogleAnalyticsFilter Cannot set GaData for Provider");
        }

        base.OnActionExecuting(filterContext);
    }
}