using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions
{
    public abstract class RecruitException : Exception
    {
        protected RecruitException(string message) : base(message) { }

        protected RecruitException(string message, VacancyUser user, IDictionary<string,string> data) : base(message)
        {
            Data.Add("UserName", user.Name);
            foreach(var item in data)
            {
                Data.Add(item.Key, item.Value);
            }
        }
    }
}
