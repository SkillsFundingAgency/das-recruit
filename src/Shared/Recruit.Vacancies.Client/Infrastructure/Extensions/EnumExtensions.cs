using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<ApplicationReviewStatus> GetSharedApplicationReviewStatusesForEmployer(this ApplicationReviewStatus status)
        {
            return new List<ApplicationReviewStatus>()
            {
                ApplicationReviewStatus.EmployerInterviewing,
                ApplicationReviewStatus.EmployerUnsuccessful,
                ApplicationReviewStatus.Shared
            };
        }
    }
}
