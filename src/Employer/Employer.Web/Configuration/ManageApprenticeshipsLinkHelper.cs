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

        public string AccountHome => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsHomeRoute}";
        public string RenameAccount => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteRenameAccountRoute}";
        public string ChangePassword => $"{_externalLinks.EmployerIdamsSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteChangePasswordRoute}";
        public string ChangeEmail => $"{_externalLinks.EmployerIdamsSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteChangeEmailAddressRoute}";
        public string Finance => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsFinanceRoute}";
        public string Agreements => $"{_externalLinks.ManageApprenticeshipSiteUrl}{ManageApprenticeshipsRoutes.ManageApprenticeshipSiteAccountsAgreementsRoute}";
        public string YourTrainingProviderPermission => $"{_externalLinks.TrainingProviderPermissionUrl}{ManageApprenticeshipsRoutes.YourTrainingProviderPermissionRoute}";
        public string EmployerRecruitmentApi => $"{_externalLinks.EmployerRecruitmentApiUrl}{ManageApprenticeshipsRoutes.EmployerRecruitmentApiRoute}";

    }
}