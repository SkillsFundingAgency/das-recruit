using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions
{
    public static class ApplicationReviewQueryExtensions
    {
        public static IQueryable<ApplicationReview> Sort(this IQueryable<ApplicationReview> queryable,
            SortColumn sortColumn, SortOrder sortOrder)
        {
            var result = sortColumn switch
            {
                SortColumn.Name => sortOrder == SortOrder.Ascending
                    ? queryable.OrderBy(x => x.Application.FullName)
                    : queryable.OrderByDescending(x => x.Application.FullName),
                SortColumn.ApplicationID => sortOrder == SortOrder.Ascending
                    ? queryable.OrderBy(x => x.Id)
                    : queryable.OrderByDescending(x => x.Id),
                SortColumn.DateApplied => sortOrder == SortOrder.Ascending
                    ? queryable.OrderBy(x => x.SubmittedDate)
                    : queryable.OrderByDescending(x => x.SubmittedDate),
                SortColumn.Status => sortOrder == SortOrder.Ascending
                    ? queryable.OrderByStatus()
                    : queryable.OrderByStatusDescending(),
                _ => null
            };

            if (result == null)
            {
                throw new InvalidOperationException("Invalid SortOrder");
            }

            return result;
        }

        private static IOrderedQueryable<ApplicationReview> OrderByStatus(this IQueryable<ApplicationReview> applications)
        {
            return applications.OrderBy(x => x.Status == ApplicationReviewStatus.Successful ? 0
                : x.Status == ApplicationReviewStatus.New ? 1
                : x.Status == ApplicationReviewStatus.Shared ? 2
                : x.Status == ApplicationReviewStatus.EmployerInterviewing ? 3
                : x.Status == ApplicationReviewStatus.Interviewing ? 3
                : x.Status == ApplicationReviewStatus.InReview ? 4
                : x.Status == ApplicationReviewStatus.EmployerUnsuccessful ? 5
                : x.Status == ApplicationReviewStatus.Unsuccessful ? 5
                : 6);
        }

        private static IOrderedQueryable<ApplicationReview> OrderByStatusDescending(this IQueryable<ApplicationReview> applications)
        {
            return applications.OrderByDescending(x => x.Status == ApplicationReviewStatus.Unsuccessful ? 0
                : x.Status == ApplicationReviewStatus.EmployerUnsuccessful ? 0
                : x.Status == ApplicationReviewStatus.InReview ? 1
                : x.Status == ApplicationReviewStatus.Interviewing ? 2
                : x.Status == ApplicationReviewStatus.EmployerInterviewing ? 2
                : x.Status == ApplicationReviewStatus.Shared ? 3
                : x.Status == ApplicationReviewStatus.New ? 4
                : x.Status == ApplicationReviewStatus.Successful ? 5
                : 6);
        }
    }
}
