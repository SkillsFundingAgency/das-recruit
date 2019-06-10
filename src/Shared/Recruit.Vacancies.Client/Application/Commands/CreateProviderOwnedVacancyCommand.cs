﻿using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateProviderOwnedVacancyCommand : ICommand, IRequest
    {
        public CreateProviderOwnedVacancyCommand(Guid vacancyId, SourceOrigin origin, long ukprn,
            string employerAccountId, VacancyUser user, UserType userType, string title)
        {
            VacancyId = vacancyId;
            Origin = origin;
            Ukprn = ukprn;
            EmployerAccountId = employerAccountId;
            User = user;
            UserType = userType;
            Title = title;
        }

        public Guid VacancyId { get; }
        public SourceOrigin Origin { get; }
        public long Ukprn { get; }
        public string EmployerAccountId { get; }
        public VacancyUser User { get; }
        public UserType UserType { get; }
        public string Title { get; }
    }
}
