using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.AboutEmployer
{
    public class AboutEmployerViewModel
    {
        public string Title { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerTitle { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public bool IsAnonymous { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(AboutEmployerEditModel.EmployerDescription),
            nameof(AboutEmployerEditModel.EmployerWebsiteUrl)
        };

        public bool IsTaskListCompleted { get ; set ; }
    }
}
