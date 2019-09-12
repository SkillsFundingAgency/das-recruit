using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Proivder.Web.Exceptions
{
    public class ApplicationWithdrawnException : RecruitException
    {
        public Guid VacancyId { get;}

        public ApplicationWithdrawnException(string message, Guid vacancyId) : base(message)
        {
            VacancyId = vacancyId;
        }
    }
}
