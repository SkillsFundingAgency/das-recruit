using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Ioc;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder ConfigureRecruitLogging(this ILoggingBuilder builder)
    {
        //Change LogLevel to Trace whilst Debugging to output Mongo commands
        builder.AddFilter("Mongo command", LogLevel.None);

        return builder;
    }
}