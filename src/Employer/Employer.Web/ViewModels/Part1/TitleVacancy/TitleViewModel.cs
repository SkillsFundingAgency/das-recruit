using System;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TitleVacancy
{
    public class TitleViewModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }

        [FromRoute]
        [ValidGuid]
        public Guid? VacancyId { get; set; }
    }
}
