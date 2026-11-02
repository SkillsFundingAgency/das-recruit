using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

[Serializable]
public class InfrastructureException : Exception
{
    public InfrastructureException(string message) : base(message) { }
}