using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Qa.Web.Exceptions
{
    [Serializable]
    public class NotFoundException : RecruitException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}