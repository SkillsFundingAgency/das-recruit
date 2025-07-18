﻿namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public enum ApplicationReviewStatus
    {
        New,
        Successful,
        Unsuccessful,
        Shared,
        InReview,
        Interviewing,
        EmployerInterviewing,
        EmployerUnsuccessful,
        PendingShared,
        PendingToMakeUnsuccessful,
        AllShared //Used for outer api call
    }
}
