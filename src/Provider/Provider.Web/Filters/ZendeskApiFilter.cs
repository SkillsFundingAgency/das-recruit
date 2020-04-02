using Esfa.Recruit.Provider.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Provider.Web.Filters
{
    public class ZendeskApiFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller != null)
            {
                var user = controller.User.ToVacancyUser();                
                controller.ViewBag.ZendeskApiData = new ZendeskApiData
                {
                    Name = user.Name,
                    Email = user.Email,
                    Organization = user.Ukprn.HasValue ? user.Ukprn.Value.ToString() : string.Empty
                };
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
}
