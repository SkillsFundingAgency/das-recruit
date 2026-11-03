using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

public abstract class RecruitException(string message) : Exception(message);