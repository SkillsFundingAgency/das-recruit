using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class EditVacancyInfoProjectionService : IEditVacancyInfoProjectionService
    {
        private readonly IQueryStoreWriter _queryStoreWriter;

        public EditVacancyInfoProjectionService(IQueryStoreWriter queryStoreWriter)
        {
            _queryStoreWriter = queryStoreWriter;
        }

        public Task UpdateEmployerVacancyDataAsync(string employerAccountId, IEnumerable<LegalEntity> legalEntities)
        {
            return _queryStoreWriter.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);
        }
    }
}
