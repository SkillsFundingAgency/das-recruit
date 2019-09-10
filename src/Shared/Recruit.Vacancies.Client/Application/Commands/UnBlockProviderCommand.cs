using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UnBlockProviderCommand : ICommand, IRequest
    {
        public long Ukprn { get; private set; }
        public VacancyUser QaVacancyUser { get; private set; }
        public DateTime UnBlockedDate { get; private set; }
        public UnBlockProviderCommand(long ukprn, VacancyUser qaVacancyUser, DateTime unBlockedDate)
        {
            Ukprn = ukprn;
            QaVacancyUser = qaVacancyUser;
            UnBlockedDate = unBlockedDate;
        }
    }
}