using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

[Serializable]
public class InvalidStateException : RecruitException
{
    public InvalidStateException(string message) : base(message) { }
}