using Esfa.Recruit.Employer.Web.RouteModel;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview
{
    public class RejectJobAdvertViewModel : VacancyRouteModel
    {
        [Required(ErrorMessage = "Select if you want to reject this job advert")]
        public bool? RejectJobAdvert { get; set; }
    }  
}
