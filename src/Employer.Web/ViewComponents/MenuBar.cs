using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ViewComponents
{
    [ViewComponent(Name = "MenuBar")]
    public class MenuBarViewComponent : ViewComponent
    {
        private readonly IOptions<ExternalLinksConfiguration> _extLinksConfig;

        public MenuBarViewComponent(IOptions<ExternalLinksConfiguration> extLinksConfig)
        {
            _extLinksConfig = extLinksConfig;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentEmployerAccountId = RouteData.Values["employerAccountId"];

            var vm = new MenuBarViewModel
            {
                AccountsFinanceLink = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsFinanceLink, currentEmployerAccountId),
                AccountsTeamsLink = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsTeamsLink, currentEmployerAccountId),
                AccountsTeamsViewLink = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsTeamsViewLink, currentEmployerAccountId),
                AccountsAgreementLink = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsAgreementLink, currentEmployerAccountId),
                AccountsSchemesLink = string.Format(_extLinksConfig.Value.ManageApprenticeshipSiteAccountsSchemesLink, currentEmployerAccountId),
            };

            return await Task.FromResult(View(vm));
        }
    }
}
