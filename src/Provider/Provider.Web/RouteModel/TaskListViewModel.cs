using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Provider.Web.RouteModel;

public class TaskListViewModel : VacancyRouteModel
{
    [FromQuery(Name = "wizard")]
    public bool IsTaskList { get; set; }
}