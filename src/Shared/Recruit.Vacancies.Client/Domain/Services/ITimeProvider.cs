using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Services
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}
