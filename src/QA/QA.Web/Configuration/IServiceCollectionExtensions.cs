using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Qa.Web.Configuration
{
    public static class IServiceCollectionExtensions
    {
        public static void AddAuthenticationService(this IServiceCollection services, AuthenticationConfiguration authConfig)
        {
            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = WsFederationDefaults.AuthenticationScheme;
                })
                .AddWsFederation(options =>
                {
                    options.Wtrealm = authConfig.Wtrealm;
                    options.MetadataAddress = authConfig.MetaDataAddress;
                    //options.CallbackPath = "/";
                    //options.SkipUnrecognizedRequests = true;
                })
                .AddCookie();
        }
    }
}