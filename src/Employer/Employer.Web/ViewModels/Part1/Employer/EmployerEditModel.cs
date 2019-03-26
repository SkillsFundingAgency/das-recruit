using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Employer
{
    public class EmployerEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.EmployerNameValidationMessages.EmployerNameRequired)]
        public long? SelectedOrganisationId { get; set; }
    }
}
