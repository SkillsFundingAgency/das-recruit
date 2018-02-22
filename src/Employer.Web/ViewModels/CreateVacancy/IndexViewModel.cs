using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy
{
    public class IndexViewModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }
    }
}
