﻿using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.FutureProspects
{
    public class FutureProspectsViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string FutureProspects { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public bool IsTaskListCompleted { get; set; }
    }
}