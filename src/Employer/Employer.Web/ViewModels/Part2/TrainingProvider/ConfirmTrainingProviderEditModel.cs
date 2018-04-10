using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ConfirmTrainingProviderEditModel
    {
        [Required]
        [FromRoute]
        public Guid VacancyId { get; set; }

        [Required]
        [Display(Name = "UKPRN")]
        [Ukprn(ErrorMessage = ValidationMessages.TrainingProviderValidationMessages.TypeOfUkprn.UkprnFormat)]
        public string Ukprn { get; set; }
    }
}