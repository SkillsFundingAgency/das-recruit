using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Configuration
{
    public class ManageApprenticeshipsLinkHelper
    {
        private readonly ExternalLinksConfiguration _externalLinks;
        private readonly ManageApprenticeshipsRoutes _maRoutes;

        public ManageApprenticeshipsLinkHelper(IOptions<ExternalLinksConfiguration> externalLinks, IOptions<ManageApprenticeshipsRoutes> maRoutes)
        {
            _externalLinks = externalLinks.Value;
            _maRoutes = maRoutes.Value;
        }

        public string MaRoot => _externalLinks.ManageApprenticeshipSiteUrl;
        public string Help => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteHelpRoute}";
        public string Accounts => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteAccountsRoute}";
        public string RenameAccount => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteRenameAccountRoute}";
        public string ChangePassword => $"{_externalLinks.EmployerIdamsSiteUrl}{_maRoutes.ManageApprenticeshipSiteChangePasswordRoute}";
        public string ChangeEmail => $"{_externalLinks.EmployerIdamsSiteUrl}{_maRoutes.ManageApprenticeshipSiteChangeEmailAddressRoute}";
        public string Notifications => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteNotificationsRoute}";
        public string Finance => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteAccountsFinanceRoute}";
        public string Apprentices => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteAccountsApprenticesRoute}";
        public string Teams => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteAccountsTeamsViewRoute}";
        public string Agreements => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteAccountsAgreementsRoute}";
        public string Schemes => $"{_externalLinks.ManageApprenticeshipSiteUrl}{_maRoutes.ManageApprenticeshipSiteAccountsSchemesRoute}";
    }
}