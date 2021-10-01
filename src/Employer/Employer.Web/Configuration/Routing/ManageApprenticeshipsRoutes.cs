namespace Esfa.Recruit.Employer.Web.Configuration.Routing
{
    public sealed class ManageApprenticeshipsRoutes
    {
        public static string ManageApprenticeshipSiteAccountsHomeRoute => "/accounts/{0}/teams";
        public static string ManageApprenticeshipSiteHelpRoute => "/service/help";
        public static string ManageApprenticeshipSiteAccountsRoute => "/service/accounts";
        public static string ManageApprenticeshipSiteRenameAccountRoute => "/accounts/{0}/rename";
        public static string ManageApprenticeshipSiteNotificationsRoute => "/settings/notifications";
        public static string ManageApprenticeshipSiteChangePasswordRoute => "/account/changepassword?clientId={0}&returnUrl={1}";
        public static string ManageApprenticeshipSiteChangeEmailAddressRoute => "/account/changeemail?clientId={0}&returnUrl={1}";
        public static string ManageApprenticeshipSiteAccountsFinanceRoute => "/accounts/{0}/finance";
        public static string ManageApprenticeshipSiteAccountsApprenticesRoute => "/commitments/accounts/{0}/apprentices/home";
        public static string ManageApprenticeshipSiteAccountsTeamsViewRoute => "/accounts/{0}/teams/view";
        public static string ManageApprenticeshipSiteAccountsAgreementsRoute => "/accounts/{0}/agreements";
        public static string ManageApprenticeshipSiteAccountsSchemesRoute => "/accounts/{0}/schemes";
        public static string ManageApprenticeshipSiteCookieConsentRoute => "/cookieConsent/settings";
        public static string ManageApprenticeshipSiteCookieConsentRouteWithHashedAccountId => "/accounts/{0}/cookieConsent";
        public static string ManageApprenticeshipSitePrivacyRoute => "/service/privacy";
        public static string EmployerFavouritesSiteApprenticeshipListRoute => "/accounts/{0}/apprenticeships";
        public static string EmployerFavouritesSiteTrainingProvidersRoute => "/accounts/{0}/apprenticeships/{1}/providers";
        public static string YourTrainingProviderPermissionRoute => "/accounts/{0}/providers";
    }
}