using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Events
{
    public class UserSignedInEvent : EventBase, INotification
    {
        public string EmployerAccountId { get; private set; }
        
        public UserSignedInEvent(UpdateEmployerVacancyDataCommand command) : base(command)
        {
            EmployerAccountId = command.EmployerAccountId;
        }
    }
}
