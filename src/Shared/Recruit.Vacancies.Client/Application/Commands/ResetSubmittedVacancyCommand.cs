using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class ResetSubmittedVacancyCommand : ICommand, IRequest<Unit>
    {
        public Guid VacancyId { get; }

        public ResetSubmittedVacancyCommand(Guid vacancyId)
        {
            VacancyId = vacancyId;
        }
    }
}