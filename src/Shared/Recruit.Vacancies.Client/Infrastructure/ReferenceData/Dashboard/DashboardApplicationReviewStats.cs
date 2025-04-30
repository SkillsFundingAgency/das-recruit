using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Dashboard
{
    public record DashboardApplicationReviewStats
    {
        public List<ApplicationReviewStats> ApplicationReviewStatsList { get; set; } = [];

        public static implicit operator DashboardApplicationReviewStats(List<ApplicationReviewStats> source)
        {
            if (source is null) return new DashboardApplicationReviewStats();

            return new DashboardApplicationReviewStats
            {
                ApplicationReviewStatsList = source.Select(x => x).ToList()
            };
        }
    }
}