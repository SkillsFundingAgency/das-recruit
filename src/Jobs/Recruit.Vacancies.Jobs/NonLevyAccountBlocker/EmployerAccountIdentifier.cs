namespace Esfa.Recruit.Vacancies.Jobs.NonLevyAccountBlocker
{
    public struct EmployerAccountIdentifier
    {
        public string AccountId { get; private set; }
        public string HashedAccountId { get; private set; }

        public EmployerAccountIdentifier(string accountId, string hashedAccountId)
        {
            AccountId = accountId;
            HashedAccountId = hashedAccountId;
        }
    }
}