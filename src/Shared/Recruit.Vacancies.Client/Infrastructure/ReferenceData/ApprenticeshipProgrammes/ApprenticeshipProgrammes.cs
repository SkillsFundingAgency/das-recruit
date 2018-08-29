using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes
{
    public class ApprenticeshipProgrammes : IReferenceDataItem
    {
        public string Id { get; set; }
        public DateTime LastUpdatedDate { get; set; } 
        public List<ApprenticeshipProgramme> Data { get; set; }
    }
}