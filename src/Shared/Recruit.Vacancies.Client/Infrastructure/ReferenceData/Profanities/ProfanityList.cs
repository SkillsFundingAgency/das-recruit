using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;

public class ProfanityList : IReferenceDataItem
{
    public string Id { get; set; }
    public DateTime LastUpdatedDate { get; set; }

    public List<string> Profanities { get; set; }
}