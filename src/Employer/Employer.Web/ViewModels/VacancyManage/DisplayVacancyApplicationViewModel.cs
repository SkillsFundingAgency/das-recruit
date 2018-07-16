using System.Linq;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyManage;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public abstract class DisplayVacancyApplicationViewModel : DisplayVacancyViewModel
    {
        public VacancyApplicationsViewModel Applications { get; internal set; }
        
        public bool HasApplications => Applications.Applications.Any();
    }
}