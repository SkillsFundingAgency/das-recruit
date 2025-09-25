using System;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews
{
    public class ApplicationReviewsToUnsuccessfulViewModel : ApplicationReviewsToUnsuccessfulRouteModel
    {
        public List<Guid> ApplicationsToUnsuccessful { get; set; }
        public List<VacancyApplication> VacancyApplications { get; set; }
        public string ShouldMakeOthersUnsuccessfulBannerHeader { get; set; }
        public string ShouldMakeOthersUnsuccessfulBannerBody { get; set; }
        public bool CanShowShouldMakeOthersUnsuccessfulBanner => !string.IsNullOrEmpty(ShouldMakeOthersUnsuccessfulBannerHeader);
        public bool CanShowCandidateAppliedLocations => VacancyApplications?.Any(app => app.CanShowCandidateAppliedLocations) ?? false;
    }
}