using System;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers;

public interface ITimeProvider
{
    DateTime Now { get; }
    DateTime OneHour { get; }
    DateTime Today { get; }
    DateTime NextDay { get; }
    DateTime NextDay6am { get; }
}