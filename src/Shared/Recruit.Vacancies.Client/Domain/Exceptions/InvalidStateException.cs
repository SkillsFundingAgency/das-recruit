using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions
{
    [Serializable]
    public class InvalidStateException : RecruitException
    {
        public InvalidStateException(string message) 
            : base(message) { }
        public InvalidStateException(string message, VacancyUser user, IDictionary<string,string> data) 
            : base (message, user, data) {}
    }
}
