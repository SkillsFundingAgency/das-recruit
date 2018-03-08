using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateApprenticeshipProgrammesCommand : ICommand, IRequest
    {
        public IEnumerable<ApprenticeshipProgramme> ApprenticeshipProgrammes { get; set; }
    }
}
