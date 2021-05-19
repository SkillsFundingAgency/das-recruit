using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class TransferEmployerReviewToQAReviewCommand : ICommand, IRequest
    {
        public TransferEmployerReviewToQAReviewCommand(Guid vacancyId, Guid userRef, string userEmailAddress, string userName)
        {
            VacancyId = vacancyId;
            UserRef = userRef;
            UserEmailAddress = userEmailAddress;
            UserName = userName;
        }

        public Guid VacancyId { get;}
        public Guid UserRef { get; }
        public string UserEmailAddress { get; }
        public string UserName { get; }
    }
}
