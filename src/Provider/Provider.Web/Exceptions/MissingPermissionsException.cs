using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions
{
    public class MissingPermissionsException : RecruitException
    {
        public MissingPermissionsException(string message) : base(message)
        {
        }
    }
}