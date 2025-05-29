using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;

public class ApplicationReviewCandidateInfo : ApplicationReviewRouteModel
{
    public string Name { get; set; }
    public string FriendlyId { get; set; }
}