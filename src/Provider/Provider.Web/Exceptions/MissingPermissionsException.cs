using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.Provider.Web.Exceptions;

public class MissingPermissionsException(string message) : RecruitException(message);