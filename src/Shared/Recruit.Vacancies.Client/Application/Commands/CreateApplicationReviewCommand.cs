using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateApplicationReviewCommand : ICommand, IRequest<Unit>
    {
        public Domain.Entities.Application Application { get; set; }
    }
}