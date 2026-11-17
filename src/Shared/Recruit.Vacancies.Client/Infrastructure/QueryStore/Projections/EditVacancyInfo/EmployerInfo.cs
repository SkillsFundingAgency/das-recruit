using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

public class EmployerInfo
{
    public string EmployerAccountId { get; set; }
    public string Name { get; set; }
    public List<LegalEntity> LegalEntities { get; set; } 
}