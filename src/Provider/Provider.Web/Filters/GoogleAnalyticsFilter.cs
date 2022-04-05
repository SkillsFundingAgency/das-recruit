using System;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Provider.Shared.UI.Models;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Filters
{
    public class GoogleAnalyticsFilter : ActionFilterAttribute
    {
        private readonly ILogger<GoogleAnalyticsFilter> _logger;

        public GoogleAnalyticsFilter(ILogger<GoogleAnalyticsFilter> logger)
        {
            _logger = logger;
        }

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
                        UserId = user.UserId,
                        UkPrn = user.Ukprn.HasValue ? user.Ukprn.Value.ToString() : string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GoogleAnalyticsFilter Cannot set GaData for Provider");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
