using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases;

public class BannedPhraseList : IReferenceDataItem
{
    public string Id { get; set; }
    public DateTime LastUpdatedDate { get; set; }

    public List<string> BannedPhrases { get; set; }
}