using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Model;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity
{
    public class LegalEntityViewModel
    {
        public IEnumerable<OrganisationViewModel> Organisations { get; set; }

        public bool HasOnlyOneOrganisation => Organisations.Count() == 1;
        
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public PartOnePageInfoViewModel PageInfo { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(SelectedOrganisationId)
        };

        public long? SelectedOrganisationId { get; set; }

        public VacancyEmployerInfoModel VacancyEmployerInfoModel { get; set; }
    }

    public class OrganisationViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }   
}