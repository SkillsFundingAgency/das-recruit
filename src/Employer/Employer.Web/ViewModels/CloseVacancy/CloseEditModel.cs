using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class CloseEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.CloseVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmClose { get; set; }
    }
}