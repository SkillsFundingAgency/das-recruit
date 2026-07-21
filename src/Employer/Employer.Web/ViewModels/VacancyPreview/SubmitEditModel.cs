using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Preview
{
    public class SubmitEditModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "You must confirm that the information is correct before submitting it.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that the information is correct before submitting it.")]
        public bool HasUserConfirmation { get; set; }
    }
}
