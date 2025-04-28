using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Dashboard
{
    public record DashboardApplicationReviewStats
    {
        public List<ApplicationReviewStats> ApplicationReviewStatsList { get; set; } = [];

        public static implicit operator DashboardApplicationReviewStats(GetApplicationReviewsCountApiResponse source)
        {
            if (source is null) return new DashboardApplicationReviewStats();

            return new DashboardApplicationReviewStats
            {
                ApplicationReviewStatsList = source?.ApplicationReviewStatsList.Select(x => (ApplicationReviewStats)x).ToList()
            };
        }

        public record ApplicationReviewStats
        {
            public long VacancyReference { get; set; }
            public int Applications { get; set; }
            public int NewApplications { get; set; }
            public int SharedApplications { get; set; }
            public int SuccessfulApplications { get; set; }
            public int UnsuccessfulApplications { get; set; }

            public static implicit operator ApplicationReviewStats(GetApplicationReviewsCountApiResponse.ApplicationReviewStats source)
            {
                if (source is null) return null;

                return new ApplicationReviewStats
                {
                    VacancyReference = source.VacancyReference,
                    Applications = source.Applications,
                    NewApplications = source.NewApplications,
                    SharedApplications = source.SharedApplications,
                    SuccessfulApplications = source.SuccessfulApplications,
                    UnsuccessfulApplications = source.UnsuccessfulApplications
                };
            }
        }
    }
}