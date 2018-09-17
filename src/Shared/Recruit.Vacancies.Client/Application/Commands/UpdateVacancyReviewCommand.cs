using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateVacancyReviewCommand : ICommand, IRequest
    {
        public VacancyReview Review { get; internal set; }
    }
}
