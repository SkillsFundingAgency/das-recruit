using Esfa.Recruit.Vacancies.Client.Domain.Entities;
namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy
{
    public class Wage
    {
        public int Duration { get; set; }

        public string DurationUnit { get; set; }

        public string WorkingWeekDescription { get; set; }

        public decimal WeeklyHours { get; set; }

        public string WageType { get; set; }

        public decimal? FixedWageYearlyAmount { get; set; }

        public string WageAdditionalInformation { get; set; }
        public string CompetitiveSalaryType { get; set; }
    }
}