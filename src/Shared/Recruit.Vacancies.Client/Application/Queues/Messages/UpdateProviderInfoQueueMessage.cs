using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public class UpdateProviderInfoQueueMessage
{
    public List<long> Ukprns { get; set; }
}