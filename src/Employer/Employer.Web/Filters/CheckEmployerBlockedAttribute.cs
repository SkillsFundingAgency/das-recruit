using System;
using Esfa.Recruit.Employer.Web.Caching;
using Esfa.Recruit.Employer.Web.Filters;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Filters
{
    public class CheckEmployerBlockedAttribute : Attribute, IFilterFactory
    {
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var blockedEmployersProvider = (IBlockedEmployersProvider)serviceProvider.GetService(typeof(IBlockedEmployersProvider));
            var vacancyClient = (IEmployerVacancyClient)serviceProvider.GetService(typeof(IEmployerVacancyClient));
            var cache = (ICache)serviceProvider.GetService(typeof(ICache));
            var dataProtectionProvider =  (IDataProtectionProvider)serviceProvider.GetService(typeof(IDataProtectionProvider));
            var hostingEnvironment =  (IHostingEnvironment)serviceProvider.GetService(typeof(IHostingEnvironment));
            var logger = (ILogger<CheckEmployerBlockedFilter>)serviceProvider.GetService(typeof(ILogger<CheckEmployerBlockedFilter>));

            return new CheckEmployerBlockedFilter(blockedEmployersProvider, cache, vacancyClient, dataProtectionProvider, hostingEnvironment, logger);
        }

        public bool IsReusable => false;
    }
}
