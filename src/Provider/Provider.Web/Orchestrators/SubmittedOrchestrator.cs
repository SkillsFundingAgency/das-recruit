using Esfa.Recruit.Provider.Web.ViewModels.Submitted;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class SubmittedOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;


        public SubmittedOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, IUtility utility)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _utility = utility;
        }

        public async Task<VacancySubmittedConfirmationViewModel> GetVacancySubmittedConfirmationViewModelAsync(VacancyRouteModel vrm, VacancyUser vacancyUser)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync(vrm, RouteNames.Submitted_Index_Get);

            if (vacancy.Status != VacancyStatus.Submitted)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotSubmittedSuccessfully, vacancy.Title));

            var isResubmit = false;
            if (vacancy.VacancyReference.HasValue)
            {
                var review = await _vacancyClient.GetCurrentReferredVacancyReviewAsync(vacancy.VacancyReference.Value);
                isResubmit = review != null;
            }

            var preferences = await _vacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);
            
            var vm = new VacancySubmittedConfirmationViewModel
            {
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference?.ToString(),
                IsResubmit = isResubmit,
                HasNotificationsSet = preferences != null && preferences.NotificationTypes > NotificationTypes.None,
                IsVacancyRejectedByESFANotificationSelected = preferences.NotificationTypes.HasFlag(NotificationTypes.VacancyRejected),
                Ukprn = vrm.Ukprn,
                VacancyId = vrm.VacancyId
            };

            return vm;
        }
    }
}