using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Data.Common;

namespace Esfa.Recruit.Vacancies.Jobs.NonLevyAccountBlocker
{
    public sealed class AccountsReader
    {
        private const string SelectHashedAccountsSql = @"SELECT  Id
,       HashedId
FROM    [employer_account].[Account]";

        private const string SelectLevyPayerAccountsSql = @"SELECT
DISTINCT  AccountId
FROM      [employer_financial].[TransactionLine]
WHERE     TransactionType = 1
AND       Amount > 0";
        private readonly ILogger<AccountsReader> _logger;
        private readonly string _financeDbConnString;
        private readonly string _accountsDbConnString;
        private RetryPolicy RetryPolicy { get; }

        public AccountsReader(ILogger<AccountsReader> logger, string financeDbConnString, string accountsDbConnString)
        {
            _logger = logger;
            _financeDbConnString = financeDbConnString;
            _accountsDbConnString = accountsDbConnString;
            RetryPolicy = GetRetryPolicy();
        }

        public async Task<IList<string>> GetLevyPayerAccountIdsAsync()
        {
            const int employerAccountIdColIndex = 0;
            var accountIdentifiers = new List<string>();

            try
            {
                using (var conn = new SqlConnection(_financeDbConnString))
                {
                    using (var command = new SqlCommand(SelectLevyPayerAccountsSql, conn))
                    {
                        using (var reader = await RetryPolicy.ExecuteAsync(async context =>
                                                    {
                                                        await conn.OpenAsync();
                                                        return await command.ExecuteReaderAsync();
                                                    }, new Context(nameof(GetLevyPayerAccountIdsAsync))))
                        {
                            while (await reader.ReadAsync())
                            {
                                accountIdentifiers.Add(reader.GetInt64(employerAccountIdColIndex).ToString());
                            }

                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payer employer account id's from Finance DB.");
                throw;
            }

            return accountIdentifiers;
        }

        public async Task<IList<EmployerAccountIdentifier>> GetEmployerAccountsAsync()
        {
            const int employerAccountIdColIndex = 0;
            const int hashedAccountIdColIndex = 1;

            var hashedAccountIdentifiers = new List<EmployerAccountIdentifier>();

            try
            {
                using (var conn = new SqlConnection(_accountsDbConnString))
                {
                    using (var command = new SqlCommand(SelectHashedAccountsSql, conn))
                    {
                        using (var reader = await RetryPolicy.ExecuteAsync(async context =>
                                                    {
                                                        await conn.OpenAsync();
                                                        return await command.ExecuteReaderAsync();
                                                    }, new Context(nameof(GetEmployerAccountsAsync))))
                        {
                            while (await reader.ReadAsync())
                            {
                                var employerAccountId = reader.GetInt64(employerAccountIdColIndex).ToString();
                                var hashedAccountId = reader.GetString(hashedAccountIdColIndex);
                                hashedAccountIdentifiers.Add(new EmployerAccountIdentifier(employerAccountId, hashedAccountId));
                            }

                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payer employer account id's from Finance DB.");
                throw;
            }

            return hashedAccountIdentifiers;
        }

        private Polly.Retry.RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<SqlException>()
                    .Or<DbException>()
                    .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4)
                    }, (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Error executing SQL Command for method {context.OperationKey} Reason: {exception.Message}. Retrying in {timeSpan.Seconds} secs...attempt: {retryCount}");
                    });
        }
    }
}