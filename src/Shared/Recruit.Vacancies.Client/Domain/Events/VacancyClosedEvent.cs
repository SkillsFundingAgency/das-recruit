﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Domain.Events
{
    public class VacancyClosedEvent : EventBase, INotification, IVacancyEvent
    {
        public string EmployerAccountId { get; set; }
        public Guid VacancyId { get; set; }
        public long VacancyReference { get; set; }
    }
}