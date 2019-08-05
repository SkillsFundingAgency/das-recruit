using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CloseVacancyCommand : ICommand, IRequest
    {
        public CloseVacancyCommand(Guid vacancyId, VacancyUser user, ClosureReason reason)
        {
            VacancyId = vacancyId;
            User = user;
            ClosureReason = reason;
        }
        public Guid VacancyId { get; internal set; }
        public VacancyUser User { get; internal set; }
        public ClosureReason ClosureReason { get; set; }
    }
}
