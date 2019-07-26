using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class TransferVacancyToLegalEntityCommand : ICommand, IRequest
    {
        public long VacancyReference { get; }
        public Guid UserRef { get; }
        public string UserEmailAddress { get; }
        public string UserName { get; }

        public TransferVacancyToLegalEntityCommand(long vacancyReference, Guid userRef, string userEmailAddress, string userName)
        {
            VacancyReference = vacancyReference;
            UserRef = userRef;
            UserEmailAddress = userEmailAddress;
            UserName = userName;
        }
    }
}