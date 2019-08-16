using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class AlertsOrchestrator
    {
        private readonly IRecruitVacancyClient _client;
        private readonly ITimeProvider _timeProvider;

        public AlertsOrchestrator(IRecruitVacancyClient client, ITimeProvider timeProvider)
        {
            _client = client;
            _timeProvider = timeProvider;
        }

        public Task DismissAlert(VacancyUser user, AlertType alertType)
        {
            return _client.UpdateUserAlertAsync(user.UserId, alertType, _timeProvider.Now);
        }
    }
}
