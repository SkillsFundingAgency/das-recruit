﻿using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages
{
    public class DeleteReportsQueueMessage
    {
        public DateTime? CreatedByScheduleDate { get; set; }
    }
}
