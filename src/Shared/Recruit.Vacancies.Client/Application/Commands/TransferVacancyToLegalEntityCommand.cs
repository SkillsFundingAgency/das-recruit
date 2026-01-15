using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class TransferVacancyToLegalEntityCommand : ICommand, IRequest<Unit>
    {
        public Guid Id { get; }
        public Guid UserRef { get; }
        public string UserEmailAddress { get; }
        public string UserName { get; }
        public TransferReason TransferReason { get; }

        public TransferVacancyToLegalEntityCommand(Guid id, Guid userRef, string userEmailAddress, string userName, TransferReason transferReason)
        {
            Id = id;
            UserRef = userRef;
            UserEmailAddress = userEmailAddress;
            UserName = userName;
            TransferReason = transferReason;
        }
    }
}