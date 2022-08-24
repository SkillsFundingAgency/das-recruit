using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateApprenticeshipProgrammesCommandHandler : IRequestHandler<UpdateApprenticeshipProgrammesCommand, Unit>
    {
        private readonly ILogger<UpdateApprenticeshipProgrammesCommandHandler> _logger;
        private readonly IApprenticeshipProgrammesUpdateService _updaterService;

        public UpdateApprenticeshipProgrammesCommandHandler(
            ILogger<UpdateApprenticeshipProgrammesCommandHandler> logger,
            IApprenticeshipProgrammesUpdateService updaterService)
        {
            _logger = logger;
            _updaterService = updaterService;
        }

        public async Task<Unit> Handle(UpdateApprenticeshipProgrammesCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Apprenticeship Programmes Reference Data");

            await _updaterService.UpdateApprenticeshipProgrammesAsync();

            _logger.LogInformation("Updated Apprenticeship Programmes Reference Data");
            
            return Unit.Value;
        }
    }
}
