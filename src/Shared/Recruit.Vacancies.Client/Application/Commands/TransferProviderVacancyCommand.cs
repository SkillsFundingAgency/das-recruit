using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class TransferProviderVacancyCommand : ICommand, IRequest<Unit>
    {
        public Guid VacancyId { get; set; }
        public VacancyUser TransferredByUser { get; internal set; }
        public DateTime TransferredDate { get; internal set; }
        public TransferReason Reason { get; internal set; }

        public TransferProviderVacancyCommand(Guid vacancyId, VacancyUser user, DateTime transferredDate, TransferReason reason)
        {
            VacancyId = vacancyId;
            TransferredByUser = user;
            TransferredDate = transferredDate;
            Reason = reason;
        }
    }
}