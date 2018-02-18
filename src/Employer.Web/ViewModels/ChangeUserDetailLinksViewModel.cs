using Esfa.Recruit.Employer.Web.Configuration;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ChangeUserDetailLinksViewModel
    {
        public string ChangePasswordLink { get; set; }
        public string ChangeEmailAddressLink { get; set; }
        
        public ChangeUserDetailLinksViewModel(IOptions<AuthenticationConfiguration> authConfig, IOptions<ExternalLinksConfiguration> extLinksConfig)
        {
            ChangePasswordLink = string.Format(extLinksConfig.Value.ManageApprenticeshipSiteChangePasswordUrl, authConfig.Value.ClientId, string.Empty);
            ChangeEmailAddressLink = string.Format(extLinksConfig.Value.ManageApprenticeshipSiteChangeEmailAddressUrl, authConfig.Value.ClientId, string.Empty);
        }
    }
}
