using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;

namespace Esfa.Recruit.Shared.Web.Orchestrators
{
    public class AlertsOrchestrator(ITimeProvider timeProvider, IMessaging messaging)
    {
        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return messaging.SendCommandAsync(new UpdateUserAlertCommand
            {
                IdamsUserId = user.UserId,
                AlertType = alertType,
                DismissedOn = timeProvider.Now,
                DfEUserId = user.DfEUserId
            });
        }
    }
}
