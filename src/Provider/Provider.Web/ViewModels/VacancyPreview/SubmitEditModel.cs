using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview
{
    public class SubmitEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "You must confirm that the information is correct before submitting it.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that the information is correct before submitting it.")]
        public bool HasUserConfirmation { get; set; }
    }
}
