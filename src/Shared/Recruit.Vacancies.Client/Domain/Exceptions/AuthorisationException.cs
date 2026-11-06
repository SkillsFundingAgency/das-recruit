using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

[Serializable]
public class AuthorisationException(string message) : RecruitException(message);