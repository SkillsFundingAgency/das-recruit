using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview
{
    public class SubmitEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "You must confirm that the information is correct before submitting.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that the information is correct before submitting.")]
        public bool HasUserConfirmation { get; set; }
    }
}
