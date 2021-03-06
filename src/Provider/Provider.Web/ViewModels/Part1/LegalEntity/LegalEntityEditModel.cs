using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntity
{
    public class LegalEntityEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.EmployerSelectionValidationMessages.EmployerSelectionRequired)]
        public string SelectedOrganisationId { get; set; }

        public string SearchTerm { get; set; }
        public int Page { get; set; }
    }
}