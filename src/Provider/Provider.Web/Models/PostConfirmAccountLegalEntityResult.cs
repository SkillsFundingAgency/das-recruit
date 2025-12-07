using System;

namespace Esfa.Recruit.Provider.Web.Models;

public class PostConfirmAccountLegalEntityResult
{
    public Guid VacancyId { get; init; }
    public bool IsVacancyComplete { get; init; }
    public bool ShouldFlagLegalEntityChange { get; init; }
}