using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Exceptions;

public static class ExceptionMessages
{
    public const string VacancyUnauthorisedAccess = "The employer account '{0}' cannot access employer account '{1}' for vacancy '{2} ({3})'.";
    public const string VacancyUnauthorisedAccessForProvider = "The provider account '{0}' cannot access provider account '{1}' for vacancy '{2} ({3})'.";
    public const string UserIsNotTheOwner = "The {0} user is not the owner of the vacancy";
    public const string VacancyWithReferenceNotFound = "Unable to find vacancy with reference: {0}.";
    public const string VacancyWithIdNotFound = "Unable to find vacancy with id: {0}.";
    public const string ApplicationReviewUnauthorisedAccess = "The employer account '{0}' cannot access employer account '{1}' application '{2}' for vacancy '{3}'.";
    public const string ApplicationReviewUnauthorisedAccessForProvider = "The provider account '{0}' cannot access provider account '{1}' application '{2}' for vacancy '{3}'.";
    public const string ProviderEmployerAccountIdNotFound = "The provider ukprn '{0}' cannot access employer account '{1}'";

    public static readonly string UnrecognisedStatusToTransferVacancyFrom = $"{{0}} is not a recognised '{nameof(VacancyStatus)}' the vacancy {{1}} can be transferred from.";
}