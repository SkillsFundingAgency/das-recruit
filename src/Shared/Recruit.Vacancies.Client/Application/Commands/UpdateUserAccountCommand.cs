using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateUserAccountCommand : ICommand, IRequest
    {
        public string IdamsUserId { get; set; }
    }
}