using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Vacancies.Client.Domain.Enums
{
    public enum DurationUnit
    {
        [Display(Name = "Month")]
        Month,

        [Display(Name = "Year")]
        Year
    }
}
