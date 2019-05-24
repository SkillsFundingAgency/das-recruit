using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages
{
    public struct UpdateApprenticeshipProgrammesQueueMessage
    {
        public DateTime? CreatedByScheduleDate { get; set; }
    }
}
