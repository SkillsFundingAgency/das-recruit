using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Routes;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

public class ApprenticeshipRoutes : IReferenceDataItem
{
    public string Id { get; set; }
    public DateTime LastUpdatedDate { get; set; } 
    public List<ApprenticeshipRoute> Data { get; set; }
}