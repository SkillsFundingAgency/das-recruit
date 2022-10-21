using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SetupEmployerCommand : ICommand, IRequest<Unit>
    {
        public string EmployerAccountId { get; set; }
    }
}