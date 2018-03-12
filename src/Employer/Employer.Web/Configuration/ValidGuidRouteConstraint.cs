using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public class ValidGuidRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out object value) && value != null)
            {
                var guid = new Guid(value.ToString());
                return guid != Guid.Empty;
            }
            return false;
        }
    }
}