using System;
using Esfa.Recruit.Vacancies.Client.Application.Providers;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services;

public class CurrentUtcTimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.UtcNow;
    public DateTime OneHour => Now.AddHours(1);
    public DateTime Today => DateTime.Today.ToUniversalTime();
    public DateTime NextDay => Today.AddDays(1);
    public DateTime NextDay6am => NextDay.AddHours(6);
}