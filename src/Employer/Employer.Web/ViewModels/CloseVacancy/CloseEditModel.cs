using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class CloseEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.CloseVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmClose { get; set; }
    }
}