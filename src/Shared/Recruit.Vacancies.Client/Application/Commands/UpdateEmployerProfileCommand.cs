using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateEmployerProfileCommand : ICommand, IRequest<Unit>
    {
        public EmployerProfile Profile { get; set; }
        public VacancyUser User { get; set; }
    }
}
