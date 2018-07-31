using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities
{
    public class CandidateSkills : IReferenceDataItem
    {
        public string Id { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public List<string> Skills { get; set; }
    }
}
