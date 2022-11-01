using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class EmployerContactDetailsViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string EmployerContactName { get; internal set; }
        public string EmployerContactEmail { get; internal set; }
        public string EmployerContactPhone { get; internal set; }
        public string EmployerTitle { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(EmployerContactDetailsEditModel.EmployerContactName),
            nameof(EmployerContactDetailsEditModel.EmployerContactEmail),
            nameof(EmployerContactDetailsEditModel.EmployerContactPhone)
        };

        public bool IsTaskListCompleted { get ; set ; }
    }
}
