using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions
{
    public class NoEOIAgreementException : RecruitException
    {
        public NoEOIAgreementException(string message) : base(message) { }
    }
}