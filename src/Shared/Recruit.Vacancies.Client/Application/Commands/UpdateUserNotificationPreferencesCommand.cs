using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class UpdateUserNotificationPreferencesCommand : ICommand, IRequest<Unit>
    {
        public UserNotificationPreferences UserNotificationPreferences { get; set; }
    }
}