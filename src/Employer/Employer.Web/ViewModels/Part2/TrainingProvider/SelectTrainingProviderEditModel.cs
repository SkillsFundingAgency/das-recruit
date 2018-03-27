using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class SelectTrainingProviderEditModel
    {
        [FromRoute]
        public Guid VacancyId { get; set; }

        [Required]
        [Display(Name = "UKPRN")]
        public string Ukprn { get; set; }
    }
}