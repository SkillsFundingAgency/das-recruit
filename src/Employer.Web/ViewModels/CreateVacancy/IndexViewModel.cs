using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ViewModels.CreateVacancy
{
    public class IndexViewModel
    {
        [Required]
        public string Title { get; set; }
    }
}
