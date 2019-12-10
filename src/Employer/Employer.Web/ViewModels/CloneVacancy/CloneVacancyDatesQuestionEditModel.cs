using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.CloneVacancy
{
    public class CloneVacancyDatesQuestionEditModel: VacancyRouteModel
    {
        [Required(ErrorMessage = ValMsg.ValidationMessages.CloneVacancyConfirmationMessages.SelectionRequired)]
        public bool? HasConfirmedClone { get; set; }
    }
}