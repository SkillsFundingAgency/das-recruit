using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer
{
    public class EmployersViewModel
    {
        public string SelectedEmployerId { get; set; }
        public IEnumerable<EmployerViewModel> Employers { get; set; }
    }
}