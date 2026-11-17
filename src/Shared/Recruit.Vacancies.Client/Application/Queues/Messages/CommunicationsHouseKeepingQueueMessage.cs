using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public class CommunicationsHouseKeepingQueueMessage
{
    public DateTime? CreatedByScheduleDate { get; set; }
}