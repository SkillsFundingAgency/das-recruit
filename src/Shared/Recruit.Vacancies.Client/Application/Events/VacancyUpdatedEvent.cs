﻿using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Events
{
    public class VacancyUpdatedEvent : EventBase, INotification
    {
        public string EmployerAccountId { get; set; }
    }
}