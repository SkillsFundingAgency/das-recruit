using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public class ProviderApprenticeshipsLinkHelper
    {
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly ProviderApprenticeshipsRoutes _pasRoutes;

        public ProviderApprenticeshipsLinkHelper(IOptions<ExternalLinksConfiguration> externalLinks, IOptions<ProviderApprenticeshipsRoutes> pasRoutes)
        {
            _externalLinks = externalLinks.Value;
            _pasRoutes = pasRoutes.Value;
        }
        public string AccountHome => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteAccountsHomeRoute}";
        public string Notifications => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteNotificationSettingsRoute}";
        public string Apprentices => $"{_externalLinks.CommitmentsSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteManageApprenticesRoute}";
        public string YourCohorts => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteYourCohortsRoute}";
        public string ManageFunding => $"{_externalLinks.ReservationsSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteManageFundingRoute}";
        public string Agreements => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteOrganisationAgreementsRoute}";
        public string CookieDetails => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteCookieDetails}";
        public string CookieSettings => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteCookieSettings}";
        public string Help => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteHelp}";
        public string Feedback => $"{_externalLinks.ProviderApprenticeshipSiteFeedbackUrl}";
        public string Privacy => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSitePrivacy}";
        public string TermsAndConditions => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{_pasRoutes.ProviderApprenticeshipSiteTermsAndConditions}";
    }
}