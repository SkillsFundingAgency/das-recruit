using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews
{
    public interface IApplicationReviewsEditModel
    {
        public bool IsMultipleApplications { get; }
        public string CandidateFeedback { get; }
    }
}