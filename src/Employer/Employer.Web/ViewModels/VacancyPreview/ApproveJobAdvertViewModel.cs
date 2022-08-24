using Esfa.Recruit.Employer.Web.RouteModel;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class ApproveJobAdvertViewModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "Select if you want to submit this job advert")]
        public bool? ApproveJobAdvert { get; set; }
    }
}
