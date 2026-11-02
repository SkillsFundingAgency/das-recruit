using System;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

[Serializable]
public class ReportNotFoundException : RecruitException
{
    public ReportNotFoundException(string message) : base(message) { }
}