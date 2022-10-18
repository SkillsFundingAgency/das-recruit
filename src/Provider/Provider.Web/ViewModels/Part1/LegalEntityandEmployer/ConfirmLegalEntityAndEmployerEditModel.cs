using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer
{
    public class ConfirmLegalEntityAndEmployerEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "Select if you want to use these details")]
        public bool? HasConfirmedEmployer { get; set; }
        public string EmployerAccountId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string EmployerName { get; set; }
    }
}