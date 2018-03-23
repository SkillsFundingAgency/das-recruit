using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Vacancies.Client.Domain
{
    public enum TrainingType
    {
        [Display(Name = "Standard")]
        Standard = 0,
        [Display(Name = "Framework")]
        Framework = 1
    }
}
