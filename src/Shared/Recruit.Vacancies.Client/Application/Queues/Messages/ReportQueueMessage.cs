using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public class ReportQueueMessage
{
    public Guid ReportId { get; set; }
}