using System;
using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;

public class ApplicationReviewsToUnsuccessfulViewModel : ApplicationReviewsToUnsuccessfulRouteModel
{
    public long VacancyReference { get; set; }
    public List<VacancyApplication> VacancyApplications { get; set; }
    public string PositionsFilledBannerHeader { get; set; }
    public string PositionsFilledBannerBody { get; set; }
    public bool CanShowPositionsFilledBanner => !string.IsNullOrEmpty(PositionsFilledBannerHeader);
    public List<Guid> ApplicationsToUnsuccessful { get; set; }
}