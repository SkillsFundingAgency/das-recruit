using System;
using Esfa.Recruit.Employer.Web.Caching;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Employer.Web.Filters
{
    public class CheckEmployerBlockedAttribute : Attribute, IFilterFactory
    {
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var blockedEmployersProvider = (IBlockedEmployersProvider)serviceProvider.GetService(typeof(IBlockedEmployersProvider));
            var cache = (ICache)serviceProvider.GetService(typeof(ICache));

            return new CheckEmployerBlockedFilter(blockedEmployersProvider, cache);
        }

        public bool IsReusable => false;
    }
}
