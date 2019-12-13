using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages
{
    public class DataMigrationQueueMessage
    {
        public Guid VacancyId { get; set; }
    }
}