using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application;

public static class ApplicationReviewStatusExtensions
{
    public static string GetDefaultCandidateFeedback(this ApplicationReviewStatus status)
    {
        return status switch
        {
            ApplicationReviewStatus.Unsuccessful => Constants.DefaultCandidateFeedback,
            _ => null
        };
    }
}