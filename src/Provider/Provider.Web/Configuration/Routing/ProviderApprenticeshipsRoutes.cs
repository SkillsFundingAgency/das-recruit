namespace Esfa.Recruit.Provider.Web.Configuration.Routing
{
    public sealed class ProviderApprenticeshipsRoutes
    {
        public static string ProviderApprenticeshipSiteAccountsHomeRoute => "/account";
        public static string ProviderApprenticeshipSiteManageApprenticesRoute => "/{0}/apprentices";
        public static string ProviderApprenticeshipSiteYourCohortsRoute => "/{0}/unapproved";
        public static string ProviderApprenticeshipSiteManageFundingRoute => "/{0}/reservations/manage";
        public static string ProviderApprenticeshipSiteOrganisationAgreementsRoute => "/{0}/agreements";
        public static string ProviderApprenticeshipSiteNotificationSettingsRoute => "/notification-settings";
        public static string ProviderApprenticeshipSiteCookieDetails => "/cookie-details";
        public static string ProviderApprenticeshipSiteCookieSettings => "/cookies";
        public static string ProviderApprenticeshipSiteHelp => "/help";
        public static string ProviderApprenticeshipSitePrivacy => "/privacy";
        public static string ProviderApprenticeshipSiteTermsAndConditions => "/terms";
        public static string ProviderRecruitmentApi => "/{0}/recruitment/api";
    }
}