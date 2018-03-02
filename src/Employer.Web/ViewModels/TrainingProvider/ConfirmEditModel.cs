using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider
{
    public class ConfirmEditModel
    {
        [Required]
        [FromRoute]
        public Guid VacancyId { get; set; }

        [Required(ErrorMessage = "An error occured with the chosen training provider.")]
        public string ProviderName { get; set; }

        [Required(ErrorMessage = "An error occured with the chosen training provider.")]
        public string ProviderAddress { get; set; }

        [Required]
        [Display(Name = "UKPRN")]
        [RegularExpression(@"^((?!(0))[0-9]{8})$", ErrorMessage = "No provider found with this UK Provider Reference Number.")]
        public string Ukprn { get; set; }
    }
}
