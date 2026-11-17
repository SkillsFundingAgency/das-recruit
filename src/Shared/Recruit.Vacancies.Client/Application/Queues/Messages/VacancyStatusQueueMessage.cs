using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public class VacancyStatusQueueMessage
{
    public DateTime? CreatedByScheduleDate { get; set; }
}