using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;

public interface IReferenceDataItem
{
    string Id { get; set; }
    DateTime LastUpdatedDate { get; set; }
}