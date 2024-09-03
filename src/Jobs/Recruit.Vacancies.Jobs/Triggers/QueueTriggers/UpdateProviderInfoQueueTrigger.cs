using System.IO;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Microsoft.Azure.WebJobs;

namespace Esfa.Recruit.Vacancies.Jobs.Triggers.QueueTriggers;

public class UpdateProviderInfoQueueTrigger
{
    private readonly IMessaging _messaging;

    public UpdateProviderInfoQueueTrigger(IMessaging messaging)
    {
        _messaging = messaging;
    }
    public async Task ExecuteAsync(
        [QueueTrigger(QueueNames.UpdateProviderInfoQueueName, Connection = "QueueStorage")] UpdateProviderInfoQueueMessage message, 
        TextWriter log)
    {
        foreach (var ukprn in message.Ukprns)
        {
            await _messaging.PublishEvent(new SetupProviderEvent(ukprn));
        }
    }
}