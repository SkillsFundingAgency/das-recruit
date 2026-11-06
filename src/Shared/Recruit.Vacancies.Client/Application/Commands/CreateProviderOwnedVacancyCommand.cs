using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class CreateProviderOwnedVacancyCommand(
    Guid vacancyId,
    SourceOrigin origin,
    long ukprn,
    string employerAccountId,
    VacancyUser user,
    UserType userType,
    string title,
    string accountLegalEntityPublicHashedId,
    string legalEntityName)
    : ICommand, IRequest<Unit>
{
    public string LegalEntityName { get; set; } = legalEntityName;

    public Guid VacancyId { get; } = vacancyId;
    public SourceOrigin Origin { get; } = origin;
    public long Ukprn { get; } = ukprn;
    public string EmployerAccountId { get; } = employerAccountId;
    public VacancyUser User { get; } = user;
    public UserType UserType { get; } = userType;
    public string Title { get; } = title;
    public string AccountLegalEntityPublicHashedId { get; set; } = accountLegalEntityPublicHashedId;
}