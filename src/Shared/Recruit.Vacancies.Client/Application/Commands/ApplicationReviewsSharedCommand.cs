using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApplicationReviewsSharedCommand : ICommand, IRequest<Unit>
    {
        public IEnumerable<Guid> ApplicationReviewIds { get; set; }
        public VacancyUser User { get; set; }
    }
}