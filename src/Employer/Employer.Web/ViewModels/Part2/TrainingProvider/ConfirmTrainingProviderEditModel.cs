using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using System.ComponentModel.DataAnnotations;
using ValMsg = Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ConfirmTrainingProviderEditModel : VacancyRouteModel
    {
        [Required]
        [Display(Name = "UKPRN")]
        [Ukprn(ErrorMessage = ValMsg.ValidationMessages.TrainingProviderValidationMessages.TypeOfUkprn.UkprnFormat)]
        public string Ukprn { get; set; }
    }
}