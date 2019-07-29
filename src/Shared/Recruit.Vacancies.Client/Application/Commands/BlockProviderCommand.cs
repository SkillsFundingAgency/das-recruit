using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class BlockProviderCommand : ICommand, IRequest
    {
        public long Ukprn { get; set; }
        public VacancyUser QaVacancyUser { get; set; }
        public DateTime BlockedDate { get; set; }
        public string Reason { get; set; }
    }
}