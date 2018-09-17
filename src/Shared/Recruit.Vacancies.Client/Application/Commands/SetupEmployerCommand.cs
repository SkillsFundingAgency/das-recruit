﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class SetupEmployerCommand : ICommand, IRequest
    {
        public string EmployerAccountId { get; set; }
    }
}