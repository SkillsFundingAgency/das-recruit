using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public enum ProgrammeLevel
    {
        Unknown = 0,
        Intermediate = 2,
        Advanced = 3,
        Higher = 4,
        [Obsolete("Use Higher")]
        FoundationDegree = 5,
        Degree = 6,
        [Obsolete("Use Degree")]
        Masters = 7
    }
}
