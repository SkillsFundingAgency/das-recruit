﻿using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages
{
    public class UpdateBlockedEmployersQueueMessage
    {
        public DateTime? CreatedByScheduleDate { get; set; }
    }
}
