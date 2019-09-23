using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UnblockProviderCommand : ICommand, IRequest
    {
        public long Ukprn { get; private set; }
        public VacancyUser QaVacancyUser { get; private set; }
        public DateTime UnblockedDate { get; private set; }
        public UnblockProviderCommand(long ukprn, VacancyUser qaVacancyUser, DateTime unblockedDate)
        {
            Ukprn = ukprn;
            QaVacancyUser = qaVacancyUser;
            UnblockedDate = unblockedDate;
        }
    }
}