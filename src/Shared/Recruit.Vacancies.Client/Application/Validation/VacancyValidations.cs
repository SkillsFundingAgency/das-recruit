using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation
{
    [Flags]
    public enum VacancyValidations : long
    {
        None = 0x0,
        Title = 0x1,
        TrainingProvider = 0x2,
        Organisation = 0x4,
        Description = 0x8,
        Training = 0x10,
        Wages = 0x20,
        Role = 0x40,
        All = ~None,
    }
}