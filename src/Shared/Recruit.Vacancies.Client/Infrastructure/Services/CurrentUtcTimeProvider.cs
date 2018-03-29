using System;
using Esfa.Recruit.Vacancies.Client.Domain.Services;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public class CurrentUtcTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
