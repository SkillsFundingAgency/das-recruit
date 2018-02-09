using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Employer.Web.ViewModels.RoleDescription
{
    public class IndexEditModel
    {
        [Required]
        public string Title { get; set; }
    }
}
