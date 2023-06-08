using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public class ShareMultipleApplicationsPostRequest
    {
        public List<VacancyApplication> ApplicationReviewsToShare { get; set; }
    }
}
