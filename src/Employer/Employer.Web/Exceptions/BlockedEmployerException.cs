using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Employer.Web.Exceptions
{
    public class BlockedEmployerException : RecruitException
    {
        public BlockedEmployerException(string message) : base(message) { }
    }
}