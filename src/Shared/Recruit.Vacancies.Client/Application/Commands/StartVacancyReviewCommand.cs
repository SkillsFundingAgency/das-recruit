using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class StartVacancyReviewCommand : CommandBase, ICommand, IRequest
    {
        public Guid ReviewId { get; internal set; }
        public VacancyUser User { get; internal set; }
    }
}
