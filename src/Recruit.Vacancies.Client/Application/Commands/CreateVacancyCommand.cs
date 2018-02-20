using Esfa.Recruit.Storage.Client.Domain.Entities;
using Esfa.Recruit.Storage.Client.Domain.Messaging;
using MediatR;
using System;

namespace Esfa.Recruit.Storage.Client.Application.Commands
{
    public class CreateVacancyCommand : ICommand, IRequest
    {
        public Vacancy Vacancy { get; } = new Vacancy { Id = Guid.NewGuid(), AuditVacancyCreated = DateTime.UtcNow };
    }
}
