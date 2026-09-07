using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerProfile;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateEmployerProfileCommandHandler(
        ILogger<UpdateEmployerProfileCommandHandler> logger,
        IEmployerProfileService employerProfileService)
        : IRequestHandler<UpdateEmployerProfileCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateEmployerProfileCommand message, CancellationToken cancellationToken)
        {
            await employerProfileService.UpdateAsync(message.Profile);

            logger.LogInformation("Update Employer profile for employer account: {employerAccountId} and " +
                                   "AccountLegalEntityPublicHashedId:{AccountLegalEntityPublicHashedId}", message.Profile.EmployerAccountId,
                                    message.Profile.AccountLegalEntityPublicHashedId);
            
            return Unit.Value;
        }
    }
}