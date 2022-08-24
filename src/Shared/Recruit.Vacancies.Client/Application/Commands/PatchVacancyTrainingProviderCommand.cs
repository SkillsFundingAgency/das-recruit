using System;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class PatchVacancyTrainingProviderCommand : ICommand, IRequest<Unit>
    {
        public Guid VacancyId { get; private set; }

        public PatchVacancyTrainingProviderCommand(Guid vacancyId)
        {
            VacancyId = vacancyId;
        }
    }
}