using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.RouteModel
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