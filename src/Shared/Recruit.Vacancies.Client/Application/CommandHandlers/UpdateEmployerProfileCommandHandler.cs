using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateEmployerProfileCommandHandler : IRequestHandler<UpdateEmployerProfileCommand>
    {
        private readonly ILogger<UpdateEmployerProfileCommandHandler> _logger;
        private readonly IEmployerProfileRepository _employerProfileRepository;
        private readonly ITimeProvider _time;

        public UpdateEmployerProfileCommandHandler(
            ILogger<UpdateEmployerProfileCommandHandler> logger,
            IEmployerProfileRepository employerProfileRepository,
            ITimeProvider time)
        {
            _employerProfileRepository = employerProfileRepository;
            _time = time;
            _logger = logger;
        }

        public async Task Handle(UpdateEmployerProfileCommand message, CancellationToken cancellationToken)
        {
            message.Profile.LastUpdatedDate = _time.Now;
            message.Profile.LastUpdatedBy = message.User;

            await _employerProfileRepository.UpdateAsync(message.Profile);

            _logger.LogInformation("Update Employer profile for employer account: {employerAccountId} and " +
                                   "AccountLegalEntityPublicHashedId:{AccountLegalEntityPublicHashedId}", message.Profile.EmployerAccountId,
                                    message.Profile.AccountLegalEntityPublicHashedId);
        }
    }
}