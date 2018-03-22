using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Services
{
    public class CurrentTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
