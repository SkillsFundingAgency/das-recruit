using System;

namespace Esfa.Recruit.Employer.Web.Exceptions
{
    [Serializable]
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
    }
}
