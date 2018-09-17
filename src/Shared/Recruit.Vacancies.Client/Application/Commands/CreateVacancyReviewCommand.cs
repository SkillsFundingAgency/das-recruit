using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateVacancyReviewCommand : ICommand, IRequest
    {
        public long VacancyReference { get; set; }
    }
}
