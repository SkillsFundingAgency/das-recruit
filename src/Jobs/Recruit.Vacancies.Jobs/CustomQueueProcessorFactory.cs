using Microsoft.Azure.WebJobs.Host.Queues;

namespace Esfa.Recruit.Vacancies.Jobs
{
    public class CustomQueueProcessorFactory : IQueueProcessorFactory
    {
        public QueueProcessor Create(QueueProcessorOptions queueProcessorOptions)
        {
            queueProcessorOptions.Queue.CreateIfNotExistsAsync().Wait();

            // return the default processor
            return new CustomQueueProcessorFactory().Create(queueProcessorOptions);
        }
    }
}
