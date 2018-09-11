using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}
