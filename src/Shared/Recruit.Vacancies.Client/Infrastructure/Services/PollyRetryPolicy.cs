using System;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public static class PollyRetryPolicy
    {
        public static Polly.Retry.RetryPolicy GetPolicy()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry([
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4)
                ]);
        }
    }
}
