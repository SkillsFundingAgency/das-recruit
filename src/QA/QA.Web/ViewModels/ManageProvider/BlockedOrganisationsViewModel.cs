using System.Collections.Generic;

namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class BlockedOrganisationsViewModel
    {
        public List<BlockedOrganisationViewModel> BlockedOrganisations { get; set; } = new List<BlockedOrganisationViewModel>();
        public bool HasBlockedOrganisations => BlockedOrganisations.Count > 0;
    }
}