using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public struct UpdateProvidersQueueMessage
{
    public DateTime? CreatedByScheduleDate { get; set; }
}