using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Messaging
{
    public interface IMessaging
    {
        Task<TResponse> SendCommandAsync<TResponse>(ICommand<TResponse> command);
    }
}
