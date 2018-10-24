using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class EditVacancyInfoProjectionService : IEditVacancyInfoProjectionService
    {
        private readonly ILogger<EditVacancyInfoProjectionService> _logger;
        private readonly IQueryStoreWriter _queryStoreWriter;

        public EditVacancyInfoProjectionService(ILogger<EditVacancyInfoProjectionService> logger, IQueryStoreWriter queryStoreWriter)
        {
            _logger = logger;
            _queryStoreWriter = queryStoreWriter;
        }

        public async Task UpdateEmployerVacancyDataAsync(string employerAccountId, IList<LegalEntity> legalEntities)
        {
            await _queryStoreWriter.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);

            _logger.LogDebug("Legal Entities inserted: {count} for Employer: {EmployerAccountId}", legalEntities.Count, employerAccountId);
        }
    }
}
