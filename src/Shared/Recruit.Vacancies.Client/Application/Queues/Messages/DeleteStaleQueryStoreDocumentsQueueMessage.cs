using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public class DeleteStaleQueryStoreDocumentsQueueMessage
{
    public DateTime? CreatedByScheduleDate { get; set; }
}