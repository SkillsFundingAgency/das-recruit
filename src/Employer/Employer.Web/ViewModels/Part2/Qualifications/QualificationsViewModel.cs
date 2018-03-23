using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : QualificationsEditModel
    {
        public List<string> QualificationTypes { get; set; }
        public List<QualificationViewModel> Qualifications { get; set; }
    }

    public class QualificationViewModel
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public string Weighting { get; set; }
    }

    
}
