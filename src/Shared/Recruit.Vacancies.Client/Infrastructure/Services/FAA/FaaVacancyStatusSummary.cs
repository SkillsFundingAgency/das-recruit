using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA
{
    // Duplicate of das-vacancyservicesshared https://github.com/SkillsFundingAgency/das-vacancyservicesshared/blob/master/src/SFA.Apprenticeships.Core/Entities/Application/Applications/VacancyStatusSummary.cs
    // Ensure you keep both classes in sync.
    public class FaaVacancyStatusSummary
    {
        // Note: Type is `int` in FAA but `long` here, to comply with ID length in this solution
        //   FAA needs to change - recorded as tech-debt in V2
        public long LegacyVacancyId { get; }
        public FaaVacancyStatuses VacancyStatus { get; }
        public DateTime ClosingDate { get; }

        public FaaVacancyStatusSummary(long legacyVacancyId, FaaVacancyStatuses vacancyStatus, DateTime closingDate)
        {
            LegacyVacancyId = legacyVacancyId;
            VacancyStatus = vacancyStatus;
            ClosingDate = closingDate;
        }
    }
}
