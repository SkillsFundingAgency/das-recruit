using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions
{
    public static class ApplicationStatusExtensions
    {

        public static string GetCssClassForApplicationReviewStatus(this ApplicationReviewStatus status)
        {
            switch (status)
            {
                case ApplicationReviewStatus.New: return "govuk-tag govuk-tag--blue";
                case ApplicationReviewStatus.Successful: return "govuk-tag govuk-tag--green";
                case ApplicationReviewStatus.Unsuccessful: return "govuk-tag govuk-tag--red";

                default:
                    return string.Empty;
            }
        }
    }
}
