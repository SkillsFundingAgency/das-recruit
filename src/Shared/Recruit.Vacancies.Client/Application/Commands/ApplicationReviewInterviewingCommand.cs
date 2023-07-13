using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using MediatR;
using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApplicationReviewInterviewingCommand : ICommand, IRequest<Unit>
    {
        public Guid ApplicationReviewId { get; set; }
        public VacancyUser User { get; set; }
    }
}
