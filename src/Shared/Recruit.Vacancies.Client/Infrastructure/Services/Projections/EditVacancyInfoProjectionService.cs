using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class EditVacancyInfoProjectionService(
        ILogger<EditVacancyInfoProjectionService> logger,
        IQueryStoreWriter queryStoreWriter)
        : IEditVacancyInfoProjectionService
    {
        public async Task UpdateEmployerVacancyDataAsync(string employerAccountId, IList<LegalEntity> legalEntities)
        {
            await queryStoreWriter.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);

            logger.LogDebug("Legal Entities inserted: {LegalEntitiesCount} for Employer: {EmployerAccountId}", legalEntities.Count, employerAccountId);
        }

        public async Task UpdateProviderVacancyDataAsync(long ukprn, IEnumerable<EmployerInfo> employers, bool hasAgreement)
        {
            await queryStoreWriter.UpdateProviderVacancyDataAsync(ukprn, employers, hasAgreement);

            logger.LogDebug("Employers inserted: {EmployersCount} for Provider: {Ukprn} has agreement:{HasAgreement}", employers.Count(), ukprn, hasAgreement);
        }

    }
}
