using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingFirstVacancyViewModel
    {
        [Required(ErrorMessage = ValidationMessages.TrainingFirstVacancyValidationMessages.HasFoundTraining)]
        public bool? HasFoundTraining { get; set; }
    }
}
