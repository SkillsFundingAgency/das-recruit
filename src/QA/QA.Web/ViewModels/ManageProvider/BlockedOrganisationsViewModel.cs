using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.QA.Web.ViewModels.ManageProvider
{
    public class BlockedOrganisationsViewModel
    {
        public List<string> BlockedProviders { get; set; }
        public long CountOfBlockedOrganisations { get; set; }
    }
}