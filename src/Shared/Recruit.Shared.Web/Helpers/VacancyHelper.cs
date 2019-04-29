using System;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Helpers
{
    public class VacancyHelper
    {
        public static LiveUpdateKind DetermineLiveUpdateKind(Vacancy vacancy, DateTime? proposedClosingDate, DateTime? proposedStartDate)
        {
            LiveUpdateKind updateKind = LiveUpdateKind.None;

            if (vacancy.ClosingDate != proposedClosingDate)
            {
                updateKind |= LiveUpdateKind.ClosingDate;
            }

            if (vacancy.StartDate != proposedStartDate)
            {
                updateKind |= LiveUpdateKind.StartDate;
            }

            return updateKind;
        }
    }
}