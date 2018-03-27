using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Vacancies.Client.Domain.Enums;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Qualification
    {
        public string QualificationType { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public QualificationWeighting Weighting { get; set; }
    }
}
