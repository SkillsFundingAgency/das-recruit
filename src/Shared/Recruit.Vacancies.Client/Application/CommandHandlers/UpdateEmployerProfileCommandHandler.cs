using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateEmployerProfileCommandHandler : IRequestHandler<UpdateEmployerProfileCommand>
    {
        private readonly ILogger<UpdateEmployerProfileCommandHandler> _logger;
        private readonly IEmployerProfileRepository _employerProfileRepository;

        public UpdateEmployerProfileCommandHandler(
            ILogger<UpdateEmployerProfileCommandHandler> logger,
            IEmployerProfileRepository employerProfileRepository)
        {
            _employerProfileRepository = employerProfileRepository;
            _logger = logger;
        }

        public async Task Handle(UpdateEmployerProfileCommand message, CancellationToken cancellationToken)
        {
            // TODO: LWA - Do we need to add last updated info?
            await _employerProfileRepository.UpdateAsync(message.Profile);

            _logger.LogInformation("Update Employer profile for employer account: {employerAccountId} and legal entity: {legalEntityId} ", message.Profile.EmployerAccountId, message.Profile.LegalEntityId);
        }
    }
}