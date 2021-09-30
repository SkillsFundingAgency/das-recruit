using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UnassignVacancyReviewCommand : ICommand, IRequest<Unit>
    {
        public Guid ReviewId { get; set; }
    }
}
