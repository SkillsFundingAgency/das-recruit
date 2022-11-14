using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class UpdateUserNotificationPreferencesCommandHandler : IRequestHandler<UpdateUserNotificationPreferencesCommand, Unit>
    {
        private readonly IUserNotificationPreferencesRepository _userNotificationPreferencesRepository;
        public UpdateUserNotificationPreferencesCommandHandler(
            IUserNotificationPreferencesRepository userNotificationPreferencesRepository)
        {
            _userNotificationPreferencesRepository = userNotificationPreferencesRepository;
        }
        public async Task<Unit> Handle(UpdateUserNotificationPreferencesCommand message, CancellationToken cancellationToken)
        {
            await _userNotificationPreferencesRepository.UpsertAsync(message.UserNotificationPreferences);
            
            return Unit.Value;
        }
    }
}