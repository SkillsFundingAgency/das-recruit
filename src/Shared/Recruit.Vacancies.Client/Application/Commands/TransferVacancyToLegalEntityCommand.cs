using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class TransferVacancyToLegalEntityCommand : ICommand, IRequest<Unit>
    {
        public long VacancyReference { get; }
        public Guid UserRef { get; }
        public string UserEmailAddress { get; }
        public string UserName { get; }
        public TransferReason TransferReason { get; }

        public TransferVacancyToLegalEntityCommand(long vacancyReference, Guid userRef, string userEmailAddress, string userName, TransferReason transferReason)
        {
            VacancyReference = vacancyReference;
            UserRef = userRef;
            UserEmailAddress = userEmailAddress;
            UserName = userName;
            TransferReason = transferReason;
        }
    }
}