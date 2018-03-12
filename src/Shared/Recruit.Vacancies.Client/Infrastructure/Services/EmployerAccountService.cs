using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly ILogger<EmployerAccountService> _logger;
        private readonly IAccountApiClient _accountApiClient;

        public EmployerAccountService(ILogger<EmployerAccountService> logger, IAccountApiClient accountApiClient)
        {
            _logger = logger;
            _accountApiClient = accountApiClient;
        }

        public async Task<IDictionary<string, EmployerIdentifier>> GetEmployerIdentifiersAsync(string userId)
        {
            try
            {
                var accounts = await _accountApiClient.GetUserAccounts(userId);

                return accounts.Select(acc => new EmployerIdentifier { AccountId = acc.HashedAccountId, EmployerName = acc.DasAccountName })
                                .ToDictionary(item => item.AccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve account information for user Id: {userId}");
                throw;
            }
        }
    }
}