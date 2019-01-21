using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class ApplicationReviewExtensions
    {
        public static string GetFriendlyId(this ApplicationReview applicationReview)
        {
            return GetFriendlyId(applicationReview.Id);
        }

        public static string GetFriendlyId(this VacancyApplication vacancyApplication)
        {
            return GetFriendlyId(vacancyApplication.ApplicationReviewId);
        }

        private static string GetFriendlyId(Guid id)
        {
            return id.ToString().Replace("-", "").Substring(0, 7).ToUpperInvariant();
        }
    }
}
