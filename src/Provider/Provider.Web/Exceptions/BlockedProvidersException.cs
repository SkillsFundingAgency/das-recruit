using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions
{
    public class BlockedProvidersException : RecruitException
    {
        public BlockedProvidersException(string message) : base(message) { }
    }
}