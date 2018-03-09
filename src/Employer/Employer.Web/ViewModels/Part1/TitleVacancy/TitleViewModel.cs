using System;
using System.ComponentModel.DataAnnotations;
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
        public Guid? VacancyId { get; set; }
    }
}
