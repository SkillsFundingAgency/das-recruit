using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class BlockedOrganisation
{
    public Guid Id { get; set; }
    public OrganisationType OrganisationType { get; set; }
    public string OrganisationId { get; set; }
    public BlockedStatus BlockedStatus { get; set; }
    public VacancyUser UpdatedByUser { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string Reason { get; set; }
}