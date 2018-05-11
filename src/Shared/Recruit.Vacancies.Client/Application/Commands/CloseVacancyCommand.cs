using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CloseVacancyCommand : CommandBase, ICommand, IRequest
    {
        public Guid VacancyId { get; set; }
    }
}
