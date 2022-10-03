using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateApprenticeshipRouteCommand : ICommand, IRequest<Unit>
    {

    }
}