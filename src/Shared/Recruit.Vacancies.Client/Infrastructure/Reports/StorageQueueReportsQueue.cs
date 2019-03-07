using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Reports
{
    class StorageQueueReportsQueue : StorageQueue.StorageQueue, IReportsQueue
    {
        public StorageQueueReportsQueue(StorageQueueConnectionDetails details) : base(details, QueueNames.ReportQueueName)
        {
        }

        public Task Add(Guid reportId)
        {
            var message = new StorageQueueReportMessage 
            {
                ReportId = reportId
            };

            return AddMessageAsync(message);
        }
        
    }
}
