using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public interface IEditVacancyInfoProjectionService
    {
        Task UpdateEmployerVacancyDataAsync(string employerAccountId, IList<LegalEntity> legalEntities);
    }
}
