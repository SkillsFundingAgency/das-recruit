using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.CloneVacancy
{
    public class CloneVacancyEditModel: VacancyRouteModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.CloneVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmClose { get; set; }
    }
}