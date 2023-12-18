using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Wage
    {
        public int? Duration { get; set; }

        public DurationUnit? DurationUnit { get; set; }

        public string WorkingWeekDescription { get; set; }

        public decimal? WeeklyHours { get; set; }

        public WageType? WageType { get; set; }

        public decimal? FixedWageYearlyAmount { get; set; }

        public string WageAdditionalInformation { get; set; }
        
        public decimal? ApprenticeMinimumWage { get; set; }

        public decimal? Under18NationalMinimumWage { get; set; }

        public decimal? Between18AndUnder21NationalMinimumWage { get; set; }

        public decimal? Between21AndUnder25NationalMinimumWage { get; set; }

        public decimal? Over25NationalMinimumWage { get; set; }
    }
}
