using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;

public class TrainingProviders : IReferenceDataItem
{
    public string Id { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public List<TrainingProvider> Data { get; set; }
}