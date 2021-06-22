using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Preview
{
    public class SubmitReviewModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "You need to submit or reject the advert")]
        public bool? SubmitToEsfa { get; set; }
    }
}
