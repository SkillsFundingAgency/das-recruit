using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;

public class ManageNotificationsRouteModel
{
    [FromRoute]
    public string EmployerAccountId { get; set; }
}