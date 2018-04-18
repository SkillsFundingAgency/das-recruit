using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.DeleteVacancy
{
    public class DeleteEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.DeleteVacancyConfirmationMessages.SelectionRequired)]
        public bool? ConfirmDeletion { get; set; }
    }
}
