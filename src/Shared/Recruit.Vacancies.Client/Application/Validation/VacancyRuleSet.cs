using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    [Flags]
    public enum VacancyRuleSet : long
    {
        None = 0x1,
        OrganisationId = 0x2,
        OrganisationAddress = 0x4,
        NumberOfPostions = 0x8,
        ShortDescription = 0x10,
        Title = 0x20,
        ClosingDate = 0x40,
        StartDate = 0x80,
        TrainingProgramme = 0x100,
        Duration = 0x200,
        WorkingWeekDescription = 0x400,
        Wage = 0x800,
        WageAdditionalInformation = 0x10000,
        All = ~None
    }


    public struct ValidationRuleSet
    {
        
    }
}
