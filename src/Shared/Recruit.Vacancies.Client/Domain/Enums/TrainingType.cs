using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Vacancies.Client.Domain.Enums
{
    public enum TrainingType
    {
        [Display(Name = "Standard")]
        Standard = 0,
        [Display(Name = "Framework")]
        Framework = 1
    }
}
