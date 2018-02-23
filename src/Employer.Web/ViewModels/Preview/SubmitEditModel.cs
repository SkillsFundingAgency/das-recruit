using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ViewModels.Preview
{
    public class SubmitEditModel
    {
        [Required]
        [FromRoute]
        public Guid VacancyId { get; set; }
    }
}
