using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Esfa.Recruit.Storage.Client.Core.Messaging
{
    public interface IMessaging
    {
        Task SendCommand(ICommand command);
    }
}
