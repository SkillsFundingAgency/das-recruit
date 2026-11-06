using System;

namespace Recruit.Vacancies.Client.Domain.Entities;

public class BlockedOrganisationSummary
{
    public string BlockedOrganisationId { get; set; }
    public DateTime BlockedDate { get; set; }
    public string BlockedByUser { get; set; }
}