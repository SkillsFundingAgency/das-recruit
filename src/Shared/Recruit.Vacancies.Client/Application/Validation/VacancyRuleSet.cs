using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    [Flags]
    public enum VacancyRuleSet : long
    {
        None = 0x1,
        EmployerName = 0x2,
        EmployerAddress = 0x4,
        NumberOfPostions = 0x8,
        ShortDescription = 0x10,
        Title = 0x20,
        ClosingDate = 0x40,
        StartDate = 0x80,
        TrainingProgramme = 0x100,
        Duration = 0x200,
        WorkingWeekDescription = 0x400,
		WeeklyHours = 0x800,
        Wage = 0x1000,
        StartDateEndDate = 0x2000,
        MinimumWage = 0x4000,
        TrainingExpiryDate = 0x8000,
        Skills = 0x10000,
        Qualifications = 0x20000,
        Description = 0x40000,
        TrainingDescription = 0x80000,
        OutcomeDescription = 0x100000,
        ApplicationInstructions = 0x200000,
        ApplicationUrl = 0x400000,
        EmployerContactDetails = 0x800000,
        All = ~None
    }
}
