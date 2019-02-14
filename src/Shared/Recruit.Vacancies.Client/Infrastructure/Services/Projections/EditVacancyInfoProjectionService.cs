using System.Collections.Generic;
using System.Linq;
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

            _logger.LogDebug($"Legal Entities inserted: {legalEntities.Count} for Employer: {employerAccountId}");
        }

        public async Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers)
        {
            await _queryStoreWriter.UpdateProviderVacancyDataAsync(ukprn, employers);

            _logger.LogDebug($"Employers inserted: {employers.Count()} for Provider: {ukprn}");
        }

    }
}
