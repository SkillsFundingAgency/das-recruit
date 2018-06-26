using System;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewEditModel : ApplicationReviewRouteModel
    {
        [Required(ErrorMessage = ValidationMessages.ApplicationReview.OutcomeRequired)]
        public ApplicationReviewStatus? Outcome { get; set; }

        public string Reason { get; set; }
    }
}
