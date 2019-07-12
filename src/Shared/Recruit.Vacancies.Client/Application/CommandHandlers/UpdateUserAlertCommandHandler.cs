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
    public class UpdateUserAlertCommandHandler : IRequestHandler<UpdateUserAlertCommand>
    {
        private readonly IRecruitVacancyClient _client;
        private readonly IUserRepository _userRepository;
        public UpdateUserAlertCommandHandler(IRecruitVacancyClient client, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _client = client;
        }

        public async Task Handle(UpdateUserAlertCommand message, CancellationToken cancellationToken)
        {
            var user = await _client.GetUsersDetailsAsync(message.IdamsUserId);

            switch (message.AlertType)
            {
                case AlertType.TransferredVacancies:
                    user.TransferredVacanciesAlertDismissedOn = message.DismissedOn;
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Cannot handle this alert dismissal {message.AlertType}");
            }

            await _userRepository.UpsertUserAsync(user);
        }
    }
}