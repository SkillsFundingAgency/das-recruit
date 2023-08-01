using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews
{
    public interface IApplicationReviewsEditModel
    {
        ApplicationReviewStatus? Outcome { get; set; }
        string CandidateFeedback { get; set; }
        public bool IsMultipleApplications { get; }
    }
}