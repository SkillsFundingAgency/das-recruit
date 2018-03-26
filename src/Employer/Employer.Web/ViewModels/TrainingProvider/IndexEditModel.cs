using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.TrainingProvider
{
    public class IndexEditModel
    {
        [FromRoute]
        public Guid VacancyId { get; set; }

        [Required]
        [Display(Name = "UKPRN")]
        public string Ukprn { get; set; }
    }
}