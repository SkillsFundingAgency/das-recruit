using System.Linq;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyView
{
    public abstract class DisplayVacancyApplicationViewModel : DisplayVacancyViewModel
    {
        public VacancyApplicationsViewModel Applications { get; internal set; }
        
        public bool HasApplications => Applications.Applications.Any();
    }
}