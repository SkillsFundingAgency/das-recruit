using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.ProviderContactDetails
{
    public class ProviderContactDetailsEditModel : VacancyRouteModel
    {
        public string ProviderContactName { get; set; }
        public string ProviderContactEmail { get; set; }
        public string ProviderContactPhone { get; set; }
        
        [Required(ErrorMessage = "Select if you want to add contact details")]
        public bool? AddContactDetails { get; set; }
    }
}
