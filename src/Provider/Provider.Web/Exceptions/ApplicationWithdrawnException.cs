using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Proivder.Web.Exceptions;

public class ApplicationWithdrawnException(string message, Guid vacancyId) : RecruitException(message)
{
    public Guid VacancyId { get;} = vacancyId;
}