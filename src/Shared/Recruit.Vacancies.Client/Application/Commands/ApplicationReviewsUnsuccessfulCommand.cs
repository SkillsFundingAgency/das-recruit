using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ApplicationReviewsUnsuccessfulCommand : ICommand, IRequest<Unit>
    {
        public IEnumerable<VacancyApplication> ApplicationReviews { get; set; }
        public string CandidateFeedback { get; set; }
        public VacancyUser User { get; set; }
    }
}