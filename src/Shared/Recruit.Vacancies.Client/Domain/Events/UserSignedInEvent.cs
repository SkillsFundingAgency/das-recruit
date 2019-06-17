using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class UserSignedInEvent : EventBase, INotification
    {
        public string IdamsUserId { get; set; }
    }
}