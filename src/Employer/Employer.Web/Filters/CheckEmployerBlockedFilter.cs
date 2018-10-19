using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Caching;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Esfa.Recruit.Employer.Web.Filters
{
    public class CheckEmployerBlockedFilter : IAsyncActionFilter
    {
        private const string BlockedEmployersCacheKey = "blockedEmployers";

        private readonly IBlockedEmployersProvider _blockedEmployersProvider;
        private readonly ICache _cache;

        private DateTime CacheAbsoluteExpiryTime => DateTime.Today.ToUniversalTime().AddDays(1);

        public CheckEmployerBlockedFilter(IBlockedEmployersProvider blockedEmployersProvider, ICache cache)
        {
            _blockedEmployersProvider = blockedEmployersProvider;
            _cache = cache;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var blockedEmployerAccountIds = await _cache.CacheAsideAsync(
                BlockedEmployersCacheKey, 
                CacheAbsoluteExpiryTime, 
                () =>  _blockedEmployersProvider.GetBlockedEmployerAccountIdsAsync());

            var accountIdFromUrl = context.RouteData.Values[RouteValues.EmployerAccountId].ToString().ToUpper();

            if (blockedEmployerAccountIds.Contains(accountIdFromUrl))
            {
                throw new BlockedEmployerException($"Employer account '{accountIdFromUrl}' is blocked");
            }

            await next();
        }
    }

}
