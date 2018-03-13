using Esfa.Recruit.Vacancies.Client.Application.QueryStore;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Jobs.ApprenticeshipProgrammes
{
    public class EmployerVacancyDataGenerator
    {
        private readonly ILogger<EmployerVacancyDataGenerator> _logger;
        private readonly IQueryStoreWriter _writer;
        private readonly IEmployerAccountService _accountSvc;

        public EmployerVacancyDataGenerator(ILogger<EmployerVacancyDataGenerator> logger, IQueryStoreWriter writer, IEmployerAccountService accountSvc)
        {
            _logger = logger;
            _writer = writer;
            _accountSvc = accountSvc;
        }

        internal async Task GenerateVacancyDataForEmployer(string employerAccountId)
        {
            // grab data for the view
            var legalEntities = await _accountSvc.GetEmployerLegalEntitiesAsync(employerAccountId);

            await _writer.UpdateEmployerVacancyDataAsync(employerAccountId, legalEntities);
        }
    }
}