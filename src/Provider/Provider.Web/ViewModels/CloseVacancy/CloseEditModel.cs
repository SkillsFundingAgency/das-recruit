using System.ComponentModel.DataAnnotations;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class CloseEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.CloseVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmClose { get; set; }
    }
}