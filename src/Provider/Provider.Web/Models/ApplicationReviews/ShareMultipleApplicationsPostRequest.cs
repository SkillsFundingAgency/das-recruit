﻿using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.Models.ApplicationReviews
{
    public class ShareMultipleApplicationsPostRequest : VacancyRouteModel
    {
        public IList<VacancyApplication> ApplicationReviewsToShare { get; set; }
        public bool ShareApplicationsConfirmed { get; set; }
    }
}