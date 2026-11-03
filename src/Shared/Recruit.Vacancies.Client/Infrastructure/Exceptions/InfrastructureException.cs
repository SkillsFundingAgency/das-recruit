using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Exceptions;

[Serializable]
public class InfrastructureException(string message) : Exception(message);