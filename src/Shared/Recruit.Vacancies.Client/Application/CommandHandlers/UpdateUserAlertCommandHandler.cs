using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserAlertCommandHandler(IRecruitVacancyClient client, IUserWriteRepository userRepository)
        : IRequestHandler<UpdateUserAlertCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateUserAlertCommand message, CancellationToken cancellationToken)
        {
            var user = string.IsNullOrEmpty(message.DfEUserId) 
                ? await client.GetUsersDetailsAsync(message.IdamsUserId) 
                : await client.GetUsersDetailsByDfEUserId(message.DfEUserId);

            switch (message.AlertType)
            {
                case AlertType.TransferredVacanciesEmployerRevokedPermission:
                    user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn = message.DismissedOn;
                    break;
                case AlertType.ClosedVacanciesBlockedProvider:
                    user.ClosedVacanciesBlockedProviderAlertDismissedOn = message.DismissedOn;
                    break;
                case AlertType.TransferredVacanciesBlockedProvider:
                    user.TransferredVacanciesBlockedProviderAlertDismissedOn = message.DismissedOn;
                    break;
                case AlertType.ClosedVacanciesWithdrawnByQa:
                    user.ClosedVacanciesWithdrawnByQaAlertDismissedOn = message.DismissedOn;
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Cannot handle this alert dismissal {message.AlertType}");
            }

            await userRepository.UpsertUserAsync(user);
            
            return Unit.Value;
        }
    }
}