using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class DeleteVacancyCommand : CommandBase, ICommand, IRequest
    {
        public Guid VacancyId { get; set; }
        public VacancyUser User { get; set; }
    }
}
