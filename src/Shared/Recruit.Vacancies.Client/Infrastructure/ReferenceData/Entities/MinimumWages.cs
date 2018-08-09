using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities
{
    public class MinimumWages : IReferenceDataItem
    {
        public string Id { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public List<MinimumWage> Ranges { get; set; }
    }
}
