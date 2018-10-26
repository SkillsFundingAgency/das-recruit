using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.NonLevyAccountBlocker
{
    public class AccountsReader
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

        public AccountsReader(ILogger<AccountsReader> logger, string financeDbConnString, string accountsDbConnString)
        {
            _logger = logger;
            _financeDbConnString = financeDbConnString;
            _accountsDbConnString = accountsDbConnString;
        }

        public async Task<IList<string>> GetLevyPayerAccountIds()
        {
            var accountIdentifiers = new List<string>();

            try
            {
                using (var conn = new SqlConnection(_financeDbConnString))
                {
                    var command = new SqlCommand(SelectLevyPayerAccountsSql, conn);

                    conn.Open();

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        //var x = reader[""].ToString();
                        accountIdentifiers.Add(reader.GetInt64(0).ToString());
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payer employer account id's from Finance DB.");
                throw;
            }

            return accountIdentifiers;
        }

        public async Task<IList<EmployerAccountIdentifier>> GetEmployerAccounts()
        {
            var hashedAccountIdentifiers = new List<EmployerAccountIdentifier>();

            try
            {
                using (var conn = new SqlConnection(_accountsDbConnString))
                {
                    var command = new SqlCommand(SelectHashedAccountsSql, conn);

                    conn.Open();

                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        hashedAccountIdentifiers.Add(new EmployerAccountIdentifier(reader.GetInt64(0).ToString(), reader.GetString(1)));
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payer employer account id's from Finance DB.");
                throw;
            }

            return hashedAccountIdentifiers;
        }
    }
}