using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class RefreshEmployerProfilesCommand : ICommand, IRequest<Unit>
{
    public string EmployerAccountId { get; set; }
    public IEnumerable<string> AccountLegalEntityPublicHashedIds { get; set; }
}