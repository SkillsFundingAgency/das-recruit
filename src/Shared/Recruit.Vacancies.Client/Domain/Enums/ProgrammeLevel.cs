using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Vacancies.Client.Domain.Enums
{
    public enum ProgrammeLevel
    {
        [Display(Name = "Unknown")]
        Unknown = 0,

        [Display(Name = "Intermediate")]
        Intermediate = 2,

        [Display(Name = "Advanced")]
        Advanced = 3,

        [Display(Name = "Higher")]
        Higher = 4,

        [Display(Name = "Foundation Degree")]
        FoundationDegree = 5,

        [Display(Name = "Degree")]
        Degree = 6,

        [Display(Name = "Masters")]
        Masters = 7
    }
}
