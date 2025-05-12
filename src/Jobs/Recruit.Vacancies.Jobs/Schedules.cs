namespace Esfa.Recruit.Vacancies.Jobs
{
    internal static class Schedules
    {
        internal const string EightAmWeeklySunday = "0 8 * * SUN";
        internal const string MidnightDaily = "0 0 * * *";
        internal const string FourAmDaily = "0 0 4 * * *";
        internal const string TwoAmDaily = "0 0 2 * * *";
        internal const string EightAmDaily = "0 0 8 * * *";
        internal const string EveryFiveMinutes = "*/5 * * * *";
        internal const string EveryFifteenMinutes = "*/15 * * * *";
        internal const string Hourly = "0 0 */1 * * *";
        internal const string WeeklyFourAmSunday = "0 4 * * SUN";
        internal const string WeeklySevenAmSunday = "0 7 * * SUN";
        internal const string WeeklyTenAmSunday = "0 10 * * SUN";
    }
}
