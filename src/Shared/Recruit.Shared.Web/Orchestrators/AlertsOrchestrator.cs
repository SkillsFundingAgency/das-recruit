using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Shared.Web.Orchestrators
{
    public class AlertsOrchestrator
    {
        private readonly IRecruitVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        private readonly IMessaging _messaging;

        public AlertsOrchestrator(IRecruitVacancyClient client, ITimeProvider timeProvider, IMessaging messaging)
        {
            _client = client;
            _timeProvider = timeProvider;
            _messaging = messaging;
        }

        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return _messaging.SendCommandAsync(new UpdateUserAlertCommand
            {
                IdamsUserId = user.UserId,
                AlertType = alertType,
                DismissedOn = _timeProvider.Now
            });
        }
    }
}
