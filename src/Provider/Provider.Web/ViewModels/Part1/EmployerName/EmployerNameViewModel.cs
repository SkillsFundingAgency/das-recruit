using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName
{
    public class EmployerNameViewModel
    {
        public string LegalEntityName { get; set; }
        public string ExistingTradingName { get; set; } 
        public string NewTradingName { get; set; }        
        public EmployerNameOptionViewModel? SelectedEmployerNameOption { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public bool HasExistingTradingName => string.IsNullOrWhiteSpace(ExistingTradingName) == false;

        public bool HasOnlyOneOrganisation { get; internal set; }
    }
}