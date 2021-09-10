using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateProvidersCommandHandler : IRequestHandler<UpdateProvidersCommand, Unit>
    {
        private readonly ILogger<UpdateProvidersCommandHandler> _logger;
        private readonly ITrainingProvidersUpdateService _updaterService;

        public UpdateProvidersCommandHandler (ILogger<UpdateProvidersCommandHandler> logger, ITrainingProvidersUpdateService updaterService)
        {
            _logger = logger;
            _updaterService = updaterService;
        }

        public async Task<Unit> Handle(UpdateProvidersCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Providers Reference Data");

            await _updaterService.UpdateProviders();

            _logger.LogInformation("Updated Providers Reference Data");
            
            return Unit.Value;
        }
    }
}