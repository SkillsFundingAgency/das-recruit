using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA
{
    // Duplicate of das-vacancyservicesshared SFA.Apprenticeships.Core/Entities/Application/Applications/VacancyStatusSummary.cs
    // Ensure you keep both classes in sync.
    public class FaaVacancyStatusSummary
    {
        // Note: Type is `int` in FAA but `long` here, to comply with ID length in this solution
        //   FAA needs to change - recorded as tech-debt in V2
        public long LegacyVacancyId { get; set; }
        public FaaVacancyStatuses VacancyStatus { get; set; }
        public DateTime ClosingDate { get; set; }

        public FaaVacancyStatusSummary() { }

        public FaaVacancyStatusSummary(long legacyVacancyId, FaaVacancyStatuses vacancyStatus, DateTime closingDate)
        {
            LegacyVacancyId = legacyVacancyId;
            VacancyStatus = vacancyStatus;
            ClosingDate = closingDate;
        }
    }
}
