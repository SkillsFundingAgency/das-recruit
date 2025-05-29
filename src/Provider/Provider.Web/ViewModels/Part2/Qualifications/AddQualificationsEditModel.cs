using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications;

public class AddQualificationsEditModel : VacancyRouteModel
{
    [Required(ErrorMessage = ValidationMessages.QualificationsConfirmationMessages.SelectionRequired)]
    public bool? AddQualificationRequirement { get; set; }
    public bool IsTaskListCompleted { get; set; }
}