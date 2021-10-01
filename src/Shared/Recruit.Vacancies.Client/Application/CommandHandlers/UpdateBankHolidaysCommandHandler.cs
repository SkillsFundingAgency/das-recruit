using Esfa.Recruit.Vacancies.Client.Application.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateBankHolidaysCommandHandler : IRequestHandler<UpdateBankHolidaysCommand, Unit>
    {
        private readonly ILogger<UpdateBankHolidaysCommandHandler> _logger;
        private readonly IBankHolidayUpdateService _updaterService;

        public UpdateBankHolidaysCommandHandler(
            ILogger<UpdateBankHolidaysCommandHandler> logger,
            IBankHolidayUpdateService updaterService)
        {
            _logger = logger;
            _updaterService = updaterService;
        }

        public async Task<Unit> Handle(UpdateBankHolidaysCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Bank Holiday Reference Data");

            await _updaterService.UpdateBankHolidaysAsync();

            _logger.LogInformation("Updated Bank Holiday Reference Data");
            
            return Unit.Value;
        }
    }
}