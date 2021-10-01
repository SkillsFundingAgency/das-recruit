using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class WithdrawApplicationCommand : ICommand, IRequest<Unit>
    {
        public Guid CandidateId { get; set; }
        public long VacancyReference { get; set; }
    }
}
