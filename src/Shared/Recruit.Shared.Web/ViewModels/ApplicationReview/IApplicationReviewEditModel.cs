using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview
{
    public interface IApplicationReviewEditModel
    {
        ApplicationReviewStatus? Outcome { get; set; }
        string CandidateFeedback { get; set; }
        bool NavigateToFeedbackPage { get; set; }
    }
}