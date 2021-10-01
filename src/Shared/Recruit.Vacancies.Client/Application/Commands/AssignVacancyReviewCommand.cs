using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class AssignVacancyReviewCommand : ICommand, IRequest<Unit>
    {
        public VacancyUser User { get; internal set; }
        public Guid? ReviewId { get; internal set; }
    }
}
