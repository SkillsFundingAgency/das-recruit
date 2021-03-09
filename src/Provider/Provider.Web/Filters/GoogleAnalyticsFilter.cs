using System;
using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
                if (controller != null)
                {
                    _logger.LogInformation($"Getting  VacancyUser Info");
                    var user = controller.User.ToVacancyUser();
                    if (user != null)
                    {
                        _logger.LogInformation($" UserId : {user.UserId}  Name {user.Name}");
                        controller.ViewBag.GaData = new GaData
                        {
                            UserId = user.UserId,
                            Acc = user.Ukprn.HasValue ? user.Ukprn.Value.ToString() : string.Empty
                        };
                    }
                }

                base.OnActionExecuting(filterContext);
            }
            catch(ArgumentNullException ex)
            {
                _logger.LogError($"GoogleAnalyticsFilter OnActionExecuting ArgumentNullException : {ex.InnerException}");
            }
            catch (Exception ex)
            {                
                _logger.LogError($"GoogleAnalyticsFilter OnActionExecuting Exception : {ex.InnerException}");
            }
        }

        public class GaData
        {
            public string UserId { get; set; }
            public string Vpv { get; set; }
            public string Acc { get; set; }
        }
    }
}
