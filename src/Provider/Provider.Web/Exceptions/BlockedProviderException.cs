using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions
{
    public class BlockedProviderException : RecruitException
    {
        public BlockedProviderException(string message) : base(message) { }
    }
}