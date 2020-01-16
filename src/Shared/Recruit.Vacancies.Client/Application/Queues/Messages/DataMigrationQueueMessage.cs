using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages
{
    public class DataMigrationQueueMessage
    {
        public int SerialNumber { get; }
        public Guid VacancyId { get; }

        public DataMigrationQueueMessage(int serialNumber, Guid vacancyId)
        {
            SerialNumber = serialNumber;
            VacancyId = vacancyId; 
        }
    }
}