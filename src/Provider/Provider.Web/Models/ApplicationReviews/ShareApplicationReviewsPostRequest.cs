using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public interface IApplicationReviewsShareModel
    {
        bool? ShareApplicationsConfirmed { get; set; }
    }

    public class ShareApplicationReviewsPostRequest : VacancyRouteModel, IApplicationReviewsShareModel
    {
        public List<VacancyApplication> ApplicationReviewsToShare { get; set; }
        public bool? ShareApplicationsConfirmed { get; set; }
    }
}