using Communication.Types;

namespace Esfa.Recruit.Vacancies.Client.Application.CommunicationPlugins
{
    public static class CommunicationExtensions
    {
        public static NotificationScope ConvertToCommunicationScope(this Domain.Entities.NotificationScope source)
        {
            return source == Domain.Entities.NotificationScope.UserSubmittedVacancies
                ? NotificationScope.Individual
                : NotificationScope.Organisation;
        }
    }
}