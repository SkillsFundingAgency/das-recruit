﻿using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider
{
    public class SelectTrainingProviderViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public string Ukprn { get; set; }
        public string TrainingProviderSearch { get; set; }
        public IEnumerable<string> TrainingProviders { get; set; }
        public bool? IsTrainingProviderSelected { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public ApprenticeshipProgrammeViewModel Programme { get; set; }
        public string BackLinkRoute { get; set; }
        public bool ReferredFromSavedFavourites { get; set; }
    }
}
