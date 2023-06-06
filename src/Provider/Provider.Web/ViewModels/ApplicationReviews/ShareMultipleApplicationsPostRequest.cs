using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ShareMultipleApplicationsPostRequest : VacancyRouteModel
    {
        public List<Guid> ApplicationsToShare { get; set; }
    }
}
