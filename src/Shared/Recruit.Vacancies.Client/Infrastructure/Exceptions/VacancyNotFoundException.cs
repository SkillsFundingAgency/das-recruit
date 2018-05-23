using System;
using System.Runtime.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions
{
    [Serializable]
    public class VacancyNotFoundException : Exception
    {
        public VacancyNotFoundException() { }
        public VacancyNotFoundException(string message) : base(message) { }

        public VacancyNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected VacancyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}