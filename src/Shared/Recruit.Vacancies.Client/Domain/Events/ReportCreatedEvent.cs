using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class ReportCreatedEvent : EventBase, INotification
    {
        public Guid ReportId { get; set; }
    }
}