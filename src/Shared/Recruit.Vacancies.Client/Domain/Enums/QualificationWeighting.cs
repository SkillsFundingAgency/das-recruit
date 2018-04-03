using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Esfa.Recruit.Vacancies.Client.Domain.Enums
{
    public enum QualificationWeighting
    {
        [Display(Name = "Essential")]
        Essential,
        [Display(Name = "Desired")]
        Desired
    }
}
