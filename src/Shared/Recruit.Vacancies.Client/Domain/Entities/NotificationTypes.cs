using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    [Flags]
    public enum NotificationTypes
    {
        None = 0,
        VacancyRejected = 1,
        VacancyClosingSoon = 1 << 1,
        ApplicationSubmitted = 1 << 2,
        VacancySentForReview = 1 << 3
    }
}