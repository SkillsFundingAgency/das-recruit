using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class AddQualificationsEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.QualificationsConfirmationMessages.SelectionRequired)]
        public bool? AddQualificationRequirement { get; set; }
    }
}
