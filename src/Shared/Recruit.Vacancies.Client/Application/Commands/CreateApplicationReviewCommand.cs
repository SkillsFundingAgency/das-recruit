using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateApplicationReviewCommand : CommandBase, ICommand, IRequest
    {
        public Domain.Entities.Application Application { get; set; }
    }
}