using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class SelectTrainingProviderEditModel : VacancyRouteModel
    {
        [Required]
        [Display(Name = "UKPRN")]
        [Ukprn(ErrorMessage = ValidationMessages.TrainingProviderValidationMessages.TypeOfUkprn.UkprnFormat)]
        public string Ukprn { get; set; }
    }
}