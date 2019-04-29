using System;

namespace Esfa.Recruit.Vacancies.Client.Application
{
    [Flags]
    public enum LiveUpdateKind
    {
        None,
        ClosingDate,
        StartDate
    }
}