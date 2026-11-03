using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Employer.Web.Exceptions;

public class ApplicationWithdrawnException(string message, Guid vacancyId) : RecruitException(message)
{
    public Guid VacancyId { get;} = vacancyId;
}