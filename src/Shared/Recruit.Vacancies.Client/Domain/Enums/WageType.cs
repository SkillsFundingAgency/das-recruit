using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Vacancies.Client.Domain.Enums
{
    public enum WageType
    {
        [Display(Name="Fixed wage")]
        FixedWage,

        [Display(Name = "National Minimum Wage for apprentices")]
        NationalMinimumWageForApprentices,

        [Display(Name = "National Minimum Wage")]
        NationalMinimumWage,

        [Display(Name = "Unspecified")]
        Unspecified
    }
}
