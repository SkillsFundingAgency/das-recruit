using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Employer.Web.ViewModels.NewVacancy
{
    public class IndexViewModel
    {
        [Required]
        public string Title { get; set; }
    }
}
