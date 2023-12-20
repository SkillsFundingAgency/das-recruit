using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;

namespace Esfa.Recruit.Vacancies.Jobs.Communication
{
    public interface INotificationService
    {
        Task Send(SendEmailCommand email);
    }
    public class NotificationService : INotificationService
    {
        private readonly IMessageSession _messageSession;

        public NotificationService(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        public Task Send(SendEmailCommand email)
        {
            return _messageSession.Send(email);
        }
    }
}