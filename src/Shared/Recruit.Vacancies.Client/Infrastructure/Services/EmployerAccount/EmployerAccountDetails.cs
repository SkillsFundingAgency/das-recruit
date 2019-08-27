namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public class EmployerAccountDetails
    {
        public AccountAgreementType AccountAgreementType { get; set; }

        public string ApprenticeshipEmployerType { get; set; }

        public EmployerAccountDetails() { }

        public EmployerAccountDetails(AccountAgreementType accountAgreementType, string apprenticeshipEmployerType)
        {
            AccountAgreementType = accountAgreementType;
            ApprenticeshipEmployerType = apprenticeshipEmployerType;
        }
    }
}
