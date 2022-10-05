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
        public string ProviderRecruitmentApi => $"{_externalLinks.ProviderRecruitmentApiUrl}{ProviderApprenticeshipsRoutes.ProviderRecruitmentApi}";
    }
}