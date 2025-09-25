using System.Collections.Generic;
using System;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.RouteModel;

public class ApplicationReviewsToUnsuccessfulRouteModel : VacancyRouteModel
{
    public ApplicationReviewStatus? Outcome { get; set; }
    public bool IsMultipleApplications { get; set; }
    
    public SortColumn SortColumn { get; set; }
    public SortOrder SortOrder { get; set; }
}

public class ApplicationReviewStatusModel
{
    public string CandidateFeedback { get; set; }
    public Guid VacancyId { get; set; }
}

public class ApplicationReviewsToUpdateStatusModel
{
    public List<Guid> ApplicationReviewIds { get; set; }
    public Guid VacancyId { get; set; }
}

public class ApplicationReviewsToUnsuccessfulRequest : VacancyRouteModel
{
    public List<Guid> ApplicationsToUnsuccessful { get; set; }
}