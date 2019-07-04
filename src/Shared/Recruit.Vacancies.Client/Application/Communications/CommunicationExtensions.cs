using Communication.Types;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications
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