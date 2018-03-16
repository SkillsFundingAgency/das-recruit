﻿using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateEmployerVacancyDataCommand : CommandBase, ICommand, IRequest
    {
        public string EmployerAccountId { get; set; }
    }
}