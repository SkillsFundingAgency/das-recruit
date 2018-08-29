using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications
{
    public class Qualifications : IReferenceDataItem
    {
        public string Id { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public List<string> QualificationTypes { get; set; }
    }
}
