using System.Collections.Generic;
using System;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.RouteModel
{
    public class ApplicationReviewsToUnsuccessfulRouteModel : VacancyRouteModel, IApplicationReviewsEditModel
    {
        public List<Guid> ApplicationsToUnsuccessful { get; set; }
        public ApplicationReviewStatus? Outcome { get; set; }
        public string CandidateFeedback { get; set; }
        public virtual bool IsMultipleApplications
        {
            get => ApplicationsToUnsuccessful != null && ApplicationsToUnsuccessful.Count > 1;
        }
    }
}
