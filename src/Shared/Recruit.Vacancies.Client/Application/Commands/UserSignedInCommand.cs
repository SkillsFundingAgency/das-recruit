﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UserSignedInCommand : CommandBase, ICommand, IRequest
    {
        public string EmployerAccountId { get; set; }
        public VacancyUser User { get; set; }
    }
}