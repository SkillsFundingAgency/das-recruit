using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Models;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer
{
    public class LegalEntityAndEmployerViewModel : VacancyRouteModel
    {
        public IEnumerable<EmployerViewModel> Employers { get; set; }
        public IEnumerable<OrganisationsViewModel> Organisations { get; internal set; }

        public string Title { get; internal set; }

        private const int LimitForNotShowingSearchPanel = 10;
        
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

        public string NoSearchResultsCaption => $"0 matches for '{SearchTerm}'";

        public int TotalNumberOfLegalEntities { get; internal set; }

        public bool IsPreviouslySelectedLegalEntityStillValid { get; internal set; }
        public bool HasPreviouslyPersistedLegalEntity => !string.IsNullOrEmpty(SelectedOrganisationId);
        public bool IsTaskListCompleted { get; set; }
        public string EmployerAccountId { get; set; }
    }


    public class EmployerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class OrganisationsViewModel
    {
        public string Id { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string EmployerName { get; set; }
        public string EmployerAccountId { get; set; }
    }
}