using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    [Flags]
    public enum VacancyRuleSet : long
    {
        None = 0,
        EmployerName = 1,
        EmployerAddress = 1 << 1,
        NumberOfPositions = 1 << 2,
        ShortDescription = 1 << 3,
        Title = 1 << 4,
        ClosingDate = 1 << 5,
        StartDate = 1 << 6,
        TrainingProgramme = 1 << 7,
        Duration = 1 << 8,
        WorkingWeekDescription = 1 << 9,
        WeeklyHours = 1 << 10,
        Wage = 1 << 11,
        StartDateEndDate = 1 << 12,
        MinimumWage = 1 << 13,
        TrainingExpiryDate = 1 << 14,
        Skills = 1 << 15,
        Qualifications = 1 << 16,
        Description = 1 << 17,
        TrainingDescription = 1 << 18,
        OutcomeDescription = 1 << 19,
        ApplicationMethod = 1 << 20,
        EmployerContactDetails = 1 << 21,
        ProviderContactDetails = 1 << 22,
        ThingsToConsider = 1 << 23,
        EmployerDescription = 1 << 24,
        EmployerWebsiteUrl = 1 << 25,
        TrainingProvider = 1 << 26,
        EmployerNameOption = 1 << 27,
        LegalEntityName = 1 << 28,
        TradingName = 1 << 29,
        WorkExperience = 1 << 30,
        RouteId = 1 << 31,
        All = ~None,
    }
}