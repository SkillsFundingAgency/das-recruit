using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.EmployerSelectionValidationMessages.EmployerSelectionRequired)]
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string SearchTerm { get; set; }
        public int Page { get; set; }
    }
}
