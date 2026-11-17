namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public enum ReviewStatus : byte
{
    New,
    PendingReview,
    UnderReview,
    Closed
}