﻿using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Provider.Web.ViewModels.VacancyView
{
    public class VacancyApplicationsViewModel : VacancyRouteModel
    {
        public List<VacancyApplication> Applications { get; internal set; }

        public bool ShowDisability { get; internal set; }

        public bool CanShowShareMultipleApplicationsLink =>
            Applications?.Any(app => app.Status == ApplicationReviewStatus.New || app.Status == ApplicationReviewStatus.InReview) ?? false;

        public bool CanShowMultipleApplicationsUnsuccessfulLink =>
          Applications?.Any(app => app.Status != ApplicationReviewStatus.Successful && app.Status != ApplicationReviewStatus.Unsuccessful) ?? false;
    }
}
