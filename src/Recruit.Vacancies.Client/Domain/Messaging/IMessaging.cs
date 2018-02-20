using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Domain.Messaging
{
    public interface IMessaging
    {
        Task SendCommandAsync(ICommand command);
    }
}
