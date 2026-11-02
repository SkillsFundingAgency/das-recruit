using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

public abstract class RecruitException : Exception
{
    protected RecruitException(string message) : base(message) { }
}