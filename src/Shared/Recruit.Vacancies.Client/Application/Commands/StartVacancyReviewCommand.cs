using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class StartVacancyReviewCommand : CommandBase, ICommand, IRequest
    {
        public Guid ReviewId { get; internal set; }
        public string UserId { get; internal set; }
    }
}
