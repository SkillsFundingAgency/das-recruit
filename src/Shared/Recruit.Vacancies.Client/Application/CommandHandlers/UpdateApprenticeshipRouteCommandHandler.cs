using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateApprenticeshipRouteCommandHandler : IRequestHandler<UpdateApprenticeshipRouteCommand, Unit>
    {
        private readonly ILogger<UpdateApprenticeshipRouteCommandHandler> _logger;
        private readonly IApprenticeshipProgrammesUpdateService _updaterService;

        public UpdateApprenticeshipRouteCommandHandler(ILogger<UpdateApprenticeshipRouteCommandHandler> logger,
            IApprenticeshipProgrammesUpdateService updaterService)
        {
            _logger = logger;
            _updaterService = updaterService;
        }

        public async Task<Unit> Handle(UpdateApprenticeshipRouteCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Apprenticeship Route Reference Data");

            await _updaterService.UpdateApprenticeshipRouteAsync();

            _logger.LogInformation("Updated Apprenticeship Route Reference Data");

            return Unit.Value;
        }
    }
}