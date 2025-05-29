using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Security.Claims;
using System;
using Esfa.Recruit.Provider.Web.Configuration;

namespace Esfa.Recruit.Provider.Web.Middleware
{
    /// <summary>
    /// Interface to define contracts related to Training Provider Authorization Handlers.
    /// </summary>
    public interface ITrainingProviderAuthorizationHandler
    {
        /// <summary>
        /// Contract to check is the logged in Provider is a valid Training Provider. 
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext.</param>
        /// <returns>boolean.</returns>
        Task<bool> IsProviderAuthorized(AuthorizationHandlerContext context);
    }

    ///<inheritdoc cref="ITrainingProviderAuthorizationHandler"/>
    public class TrainingProviderAuthorizationHandler : ITrainingProviderAuthorizationHandler
    {
        private readonly IGetProviderStatusClient _getProviderStatusClient;
        private readonly Predicate<Claim> _ukprnClaimFinderPredicate = c => c.Type.Equals(ProviderRecruitClaims.IdamsUserUkprnClaimsTypeIdentifier)
                                                                            || c.Type.Equals(ProviderRecruitClaims.DfEUkprnClaimsTypeIdentifier);

        public TrainingProviderAuthorizationHandler(
            IGetProviderStatusClient getProviderStatusClient)
        {
            _getProviderStatusClient = getProviderStatusClient;
        }

        public async Task<bool> IsProviderAuthorized(AuthorizationHandlerContext context)
        {
            long ukprn = GetProviderId(context);

            //if the ukprn is invalid return false.
            if (ukprn <= 0) return false;

            var providerStatusDetails = await _getProviderStatusClient.GetProviderStatus(ukprn);

            // Condition to check if the Provider Details has permission to access Apprenticeship Services based on the property value "CanAccessApprenticeshipService" set to True.
            return providerStatusDetails is { CanAccessService: true };
        }

        private long GetProviderId(AuthorizationHandlerContext context)
        {
            return long.TryParse(context.User.FindFirst(_ukprnClaimFinderPredicate)?.Value, out long providerId)
                ? providerId
                : 0;
        }
    }
}
