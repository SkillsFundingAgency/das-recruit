using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public class ManageApprenticeshipsLinkHelper
    {
        private readonly ExternalLinksConfiguration _externalLinks;
        
        public ManageApprenticeshipsLinkHelper(IOptions<ExternalLinksConfiguration> externalLinks)
        {
            _externalLinks = externalLinks.Value;
        }

        public string MaRoot => _externalLinks.ManageApprenticeshipSiteUrl;
        public string AccountHome => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsHomeRoute}";
        public string Help => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteHelpRoute}";
        public string Privacy => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSitePrivacyRoute}";
        public string Accounts => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsRoute}";
        public string RenameAccount => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteRenameAccountRoute}";
        public string ChangePassword => $"{_externalLinks.EmployerIdamsSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteChangePasswordRoute}";
        public string ChangeEmail => $"{_externalLinks.EmployerIdamsSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteChangeEmailAddressRoute}";
        public string Notifications => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteNotificationsRoute}";
        public string Finance => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsFinanceRoute}";
        public string Apprentices => $"{_externalLinks.CommitmentsSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsApprenticesRoute}";
        public string Teams => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsTeamsViewRoute}";
        public string Agreements => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsAgreementsRoute}";
        public string Schemes => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsSchemesRoute}";
        public string CookieConsent => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteCookieConsentRoute}";
        public string CookieConsentWithHashedAccount => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteCookieConsentRouteWithHashedAccountId}";
        public string EmployerFavouritesHome => _externalLinks.EmployerFavouritesUrl;
        public string EmployerFavouritesTrainingProviders => $"{_externalLinks.EmployerFavouritesUrl}{ManageApprenticeshipsRoutes.EmployerFavouritesSiteTrainingProvidersRoute}";
        public string EmployerFavouritesApprenticeshipList => $"{_externalLinks.EmployerFavouritesUrl}{ManageApprenticeshipsRoutes.EmployerFavouritesSiteApprenticeshipListRoute}";
        public string YourTrainingProviderPermission => $"{_externalLinks.TrainingProviderPermissionUrl}{ManageApprenticeshipsRoutes.YourTrainingProviderPermissionRoute}";
        public string EmployerRecruitmentApi => $"{_externalLinks.EmployerRecruitmentApiUrl}{ManageApprenticeshipsRoutes.EmployerRecruitmentApiRoute}";

    }
}