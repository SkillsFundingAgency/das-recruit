using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity
{
    public class LegalEntityViewModel
    {
        public string Title { get; internal set; }

        private const int LimitForNotShowingSearchPanel = 10;
        public IEnumerable<OrganisationViewModel> Organisations { get; internal set; }

        public bool HasOnlyOneOrganisation => TotalNumberOfLegalEntities == 1;

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public PartOnePageInfoViewModel PageInfo { get; internal set; }
        public PagerViewModel Pager { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(SelectedOrganisationId)
        };

        public string SelectedOrganisationId { get; internal set; }

        public VacancyEmployerInfoModel VacancyEmployerInfoModel { get; internal set; }

        public string SearchTerm { get; internal set; }
        public int Page { get; internal set; }

        public bool CanShowSearchPanel => TotalNumberOfLegalEntities > LimitForNotShowingSearchPanel;

        public bool HasNoSearchResults => string.IsNullOrEmpty(SearchTerm) == false && Organisations.Count() == 0;
        public string NoSearchResultsCaption => $"0 matches for '{SearchTerm}'";

        public int TotalNumberOfLegalEntities { get; internal set; }

        public bool IsPreviouslySelectedLegalEntityStillValid { get; internal set; }
        public bool HasPreviouslyPersistedLegalEntity => !string.IsNullOrEmpty(SelectedOrganisationId);
        public bool IsSelectedOrganisationInPagedOrganisations
                        => IsPreviouslySelectedLegalEntityStillValid
                            && HasPreviouslyPersistedLegalEntity
                            && Organisations.Any(org => org.Id == SelectedOrganisationId);

        public bool CanOutputHiddenSelectedOrganisationIdField => !string.IsNullOrEmpty(SelectedOrganisationId) && IsSelectedOrganisationInPagedOrganisations == false;
    }

    public class OrganisationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}