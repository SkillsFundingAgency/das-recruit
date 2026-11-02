using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

[Serializable]
public class VacancyNotFoundException : RecruitException
{
    public VacancyNotFoundException(string message) : base(message) { }
}