using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CloneVacancyCommand : ICommand, IRequest
    {
        public Guid IdOfVacancyToClone { get; set; }
        public Guid NewVacancyId { get; set; }
        public VacancyUser User { get; set; }
    }
}
