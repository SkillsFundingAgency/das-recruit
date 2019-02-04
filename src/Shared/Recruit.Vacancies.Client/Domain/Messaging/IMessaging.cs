﻿using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Messaging
{
    public interface IMessaging
    {
        Task SendCommandAsync(ICommand command);

        Task<T> SendCommandAsync<T>(ICommand<T> command);

        Task PublishEvent(IEvent @event);
    }
}
