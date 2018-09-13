using Esfa.Recruit.Employer.Web.Views;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class EmployerContactDetailsViewModel
    {
        public string Title { get; internal set; }
        public string EmployerContactName { get; internal set; }
        public string EmployerContactEmail { get; internal set; }
        public string EmployerContactPhone { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(EmployerContactDetailsEditModel.EmployerContactName),
            nameof(EmployerContactDetailsEditModel.EmployerContactEmail),
            nameof(EmployerContactDetailsEditModel.EmployerContactPhone)
        };
    }
}
