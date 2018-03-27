using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsEditModel : QualificationEditModel
    {
        [FromRoute]
        [Required, ValidGuid]
        public Guid VacancyId { get; set; }

        [Required]
        [FromRoute]
        public string EmployerAccountId { get; set; }
        
        public List<QualificationEditModel> Qualifications { get; set; }

        public string AddQualificationAction { get; set; }
        public string RemoveQualification { get; set; }
    }

    public class QualificationEditModel
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public string Weighting { get; set; }
    }
}
