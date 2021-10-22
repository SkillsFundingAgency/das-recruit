using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public class ProviderApprenticeshipsLinkHelper
    {
        private const string ApprovalsSubdomainPrefix = "approvals";
        private readonly ExternalLinksConfiguration _externalLinks;

        public ProviderApprenticeshipsLinkHelper(IOptions<ExternalLinksConfiguration> externalLinks)
        {
            _externalLinks = externalLinks.Value;
        }
        public string AccountHome => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteAccountsHomeRoute}";
        public string Notifications => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteNotificationSettingsRoute}";
        public string Apprentices => ApprovalsLink(ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteManageApprenticesRoute);
        public string YourCohorts => ApprovalsLink(ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteYourCohortsRoute);
        public string ManageFunding => $"{_externalLinks.ReservationsSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteManageFundingRoute}";
        public string Agreements => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteOrganisationAgreementsRoute}";
        public string CookieDetails => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteCookieDetails}";
        public string CookieSettings => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteCookieSettings}";
        public string Help => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteHelp}";
        public string Feedback => $"{_externalLinks.ProviderApprenticeshipSiteFeedbackUrl}";
        public string Privacy => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSitePrivacy}";
        public string TermsAndConditions => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteTermsAndConditions}";


        private string ApprovalsLink(string route) => ApplyPasSubdomain(ApprovalsSubdomainPrefix) + route;
        private string ApplyPasSubdomain(string subdomain)
        {
            var returnUrl = _externalLinks.ProviderApprenticeshipSiteUrl.EndsWith("/")
                ? _externalLinks.ProviderApprenticeshipSiteUrl
                : _externalLinks.ProviderApprenticeshipSiteUrl + "/";

            return returnUrl.Replace("https://", $"https://{subdomain}.");
        }
    }
}