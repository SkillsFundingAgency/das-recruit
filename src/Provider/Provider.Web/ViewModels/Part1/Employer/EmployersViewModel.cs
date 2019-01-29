using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer
{
    public class EmployersViewModel
    {
        public string SelectedEmployerId { get; set; }
        public IEnumerable<EmployerViewModel> Employers { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}