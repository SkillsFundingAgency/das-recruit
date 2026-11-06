using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;

public static class ApplicationReviewQueryExtensions
{
    public static IQueryable<Domain.Entities.ApplicationReview> Sort(this IQueryable<Domain.Entities.ApplicationReview> queryable,
        SortColumn sortColumn, SortOrder sortOrder, bool sharedTable = false)
    {
        var result = sortColumn switch
        {
            SortColumn.Default => sharedTable 
                ? queryable.OrderByResponse()
                : queryable.OrderByDefault(),
            SortColumn.Applicant => sortOrder == SortOrder.Ascending
                ? queryable.OrderBy(x => x.Application.FullName)
                : queryable.OrderByDescending(x => x.Application.FullName),
            SortColumn.Response => sortOrder == SortOrder.Ascending
                ? queryable.OrderByResponse()
                : queryable.OrderByResponseDescending(),
            SortColumn.DateReviewed => sortOrder == SortOrder.Ascending
                ? queryable.OrderBy(x => x.ReviewedDate)
                : queryable.OrderByDescending(x => x.ReviewedDate),
            SortColumn.DateShared => sortOrder == SortOrder.Ascending
                ? queryable.OrderBy(x => x.DateSharedWithEmployer)
                : queryable.OrderByDescending(x => x.DateSharedWithEmployer),
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

    private static IOrderedQueryable<Domain.Entities.ApplicationReview> OrderByResponseDescending(this IQueryable<Domain.Entities.ApplicationReview> applications)
    {
        return (IOrderedQueryable<Domain.Entities.ApplicationReview>)applications.OrderByResponse().Reverse();
    }

    private static IOrderedQueryable<Domain.Entities.ApplicationReview> OrderByResponse(this IQueryable<Domain.Entities.ApplicationReview> applications)
    {
        return applications.OrderBy(x => x.Status == ApplicationReviewStatus.New ? 0
            : x.Status == ApplicationReviewStatus.Shared ? 1
            : x.Status == ApplicationReviewStatus.Interviewing ? 3
            : x.Status == ApplicationReviewStatus.EmployerInterviewing ? 3
            : x.Status == ApplicationReviewStatus.Successful ? 5
            : x.Status == ApplicationReviewStatus.EmployerUnsuccessful ? 6
            : x.Status == ApplicationReviewStatus.Unsuccessful ? 6
            : 8).ThenBy(x => x.ReviewedDate);
    }

    private static IOrderedQueryable<Domain.Entities.ApplicationReview> OrderByDefault(this IQueryable<Domain.Entities.ApplicationReview> applications)
    {
        return applications.OrderBy(x => x.SubmittedDate).ThenBy(x => x.Status == ApplicationReviewStatus.EmployerInterviewing ? 0
            : x.Status == ApplicationReviewStatus.EmployerUnsuccessful ? 1
            : x.Status == ApplicationReviewStatus.InReview ? 2
            : x.Status == ApplicationReviewStatus.Interviewing ? 3
            : x.Status == ApplicationReviewStatus.New ? 4
            : x.Status == ApplicationReviewStatus.Shared ? 5
            : x.Status == ApplicationReviewStatus.Successful ? 6
            : x.Status == ApplicationReviewStatus.Unsuccessful ? 7
            : 8);
    }

    private static IOrderedQueryable<Domain.Entities.ApplicationReview> OrderByStatus(this IQueryable<Domain.Entities.ApplicationReview> applications)
    {
        return applications.OrderBy(x => x.Status == ApplicationReviewStatus.EmployerInterviewing ? 0
            : x.Status == ApplicationReviewStatus.EmployerUnsuccessful ? 1
            : x.Status == ApplicationReviewStatus.InReview ? 2
            : x.Status == ApplicationReviewStatus.Interviewing ? 3
            : x.Status == ApplicationReviewStatus.New ? 4
            : x.Status == ApplicationReviewStatus.Shared ? 5
            : x.Status == ApplicationReviewStatus.Successful ? 6
            : x.Status == ApplicationReviewStatus.Unsuccessful ? 7
            : 8);
    }

    private static IOrderedQueryable<Domain.Entities.ApplicationReview> OrderByStatusDescending(this IQueryable<Domain.Entities.ApplicationReview> applications)
    {
        return applications.OrderByDescending(x => x.Status == ApplicationReviewStatus.EmployerInterviewing ? 0
            : x.Status == ApplicationReviewStatus.EmployerUnsuccessful ? 1
            : x.Status == ApplicationReviewStatus.InReview ? 2
            : x.Status == ApplicationReviewStatus.Interviewing ? 3
            : x.Status == ApplicationReviewStatus.New ? 4
            : x.Status == ApplicationReviewStatus.Shared ? 5
            : x.Status == ApplicationReviewStatus.Successful ? 6
            : x.Status == ApplicationReviewStatus.Unsuccessful ? 7
            : 8);
    }
}