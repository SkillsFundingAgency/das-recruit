using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities
{
    public interface IReferenceDataItem
    {
        string Id { get; set; }
        DateTime LastUpdatedDate { get; set; }
    }
}