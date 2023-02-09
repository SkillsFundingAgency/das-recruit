using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Queues;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class CustomQueueProcessorFactory : IQueueProcessorFactory
    {
        public QueueProcessor Create(QueueProcessorFactoryContext context)
        {
            context.Queue.CreateIfNotExistsAsync().Wait();
        
            // return the default processor
            return new QueueProcessor(context);
        }
    }
}
