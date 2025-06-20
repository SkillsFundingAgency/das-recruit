using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.EmployerContactDetails;

public class EmployerContactDetailsEditModel : TaskListViewModel
{
    public string EmployerContactName { get; set; }
    public string EmployerContactEmail { get; set; }
    public string EmployerContactPhone { get; set; }
}