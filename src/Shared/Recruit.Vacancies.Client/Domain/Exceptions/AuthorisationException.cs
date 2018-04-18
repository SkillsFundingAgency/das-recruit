using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions
{
    [Serializable]
    public class AuthorisationException : Exception
    {
        public AuthorisationException(string message) : base(message) { }
    }
}
