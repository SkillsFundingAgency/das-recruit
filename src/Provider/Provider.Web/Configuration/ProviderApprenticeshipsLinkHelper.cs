using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Provider.Web.Configuration
{
    public class ProviderApprenticeshipsLinkHelper
    {
        private readonly ExternalLinksConfiguration _externalLinks;

        public ProviderApprenticeshipsLinkHelper(IOptions<ExternalLinksConfiguration> externalLinks)
        {
            _externalLinks = externalLinks.Value;
        }
        public string AccountHome => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteAccountsHomeRoute}";
        public string Notifications => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteNotificationSettingsRoute}";
        public string Apprentices => $"{_externalLinks.CommitmentsSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteManageApprenticesRoute}";
        public string YourCohorts => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteYourCohortsRoute}";
        public string ManageFunding => $"{_externalLinks.ReservationsSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteManageFundingRoute}";
        public string Agreements => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteOrganisationAgreementsRoute}";
        public string CookieDetails => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteCookieDetails}";
        public string CookieSettings => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteCookieSettings}";
        public string Help => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteHelp}";
        public string Feedback => $"{_externalLinks.ProviderApprenticeshipSiteFeedbackUrl}";
        public string Privacy => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSitePrivacy}";
        public string TermsAndConditions => $"{_externalLinks.ProviderApprenticeshipSiteUrl}{ProviderApprenticeshipsRoutes.ProviderApprenticeshipSiteTermsAndConditions}";
    }
}