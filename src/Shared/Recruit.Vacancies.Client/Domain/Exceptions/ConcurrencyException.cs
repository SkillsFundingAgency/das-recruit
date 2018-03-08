using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions
{
    [Serializable]
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
    }
}
