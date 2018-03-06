using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.Preview
{
    public class SubmitEditModel
    {
        [Required]
        [FromRoute]
        public Guid VacancyId { get; set; }
    }
}
