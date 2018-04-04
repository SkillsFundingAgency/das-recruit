using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mappings;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.EditVacancyInfo
{
    public class EditVacancyInfoUpdater
    {
        private readonly ILogger<EditVacancyInfoUpdater> _logger;
        private readonly IQueryStoreWriter _writer;
        private readonly IEmployerAccountService _accountSvc;

        public EditVacancyInfoUpdater(ILogger<EditVacancyInfoUpdater> logger, IQueryStoreWriter writer, IEmployerAccountService accountSvc)
        {
            _logger = logger;
            _writer = writer;
            _accountSvc = accountSvc;
        }

        internal async Task UpdateEditVacancyInfo(string employerAccountId)
        {
            var accountLegalEntities = await _accountSvc.GetEmployerLegalEntitiesAsync(employerAccountId);

            var legalEntities = accountLegalEntities.Select(LegalEntityMapper.MapFromAccountApiLegalEntity);

            await _writer.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);
        }
    }
}