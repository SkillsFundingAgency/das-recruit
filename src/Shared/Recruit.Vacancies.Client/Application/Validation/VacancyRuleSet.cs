using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    [Flags]
    public enum VacancyRuleSet : long
    {
        None = 0L,
        EmployerName = 1L,
        EmployerAddress = 1L << 1,
        NumberOfPositions = 1L << 2,
        ShortDescription = 1L << 3,
        Title = 1L << 4,
        ClosingDate = 1L << 5,
        StartDate = 1L << 6,
        TrainingProgramme = 1L << 7,
        Duration = 1L << 8,
        WorkingWeekDescription = 1L << 9,
        WeeklyHours = 1L << 10,
        Wage = 1L << 11,
        StartDateEndDate = 1L << 12,
        MinimumWage = 1L << 13,
        TrainingExpiryDate = 1L << 14,
        Skills = 1L << 15,
        Qualifications = 1L << 16,
        Description = 1L << 17,
        TrainingDescription = 1L << 18,
        OutcomeDescription = 1L << 19,
        ApplicationMethod = 1L << 20,
        EmployerContactDetails = 1L << 21,
        ProviderContactDetails = 1L << 22,
        ThingsToConsider = 1L << 23,
        EmployerDescription = 1L << 24,
        EmployerWebsiteUrl = 1L << 25,
        TrainingProvider = 1L << 26,
        EmployerNameOption = 1L << 27,
        LegalEntityName = 1L << 28,
        TradingName = 1L << 29,
        WorkExperience = 1L << 30,
        RouteId = 1L << 31,
        AdditionalQuestion1 = 1L << 32,
        AdditionalQuestion2 = 1L << 33,
        All = ~None,
    }
}