using Esfa.Recruit.Employer.Web.ViewModels.Submitted;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class SubmittedOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;

        public SubmittedOrchestrator(IRecruitVacancyClient vacancyClient, IUtility utility)
        {
            _vacancyClient = vacancyClient;
            _utility = utility;
        }

        public async Task<VacancySubmittedConfirmationViewModel> GetVacancySubmittedConfirmationViewModelAsync(VacancyRouteModel vrm, VacancyUser vacancyUser)
        {
            var vacancy = await _utility.GetAuthorisedVacancyAsync( vrm, RouteNames.Submitted_Index_Get);

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
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference?.ToString(),
                IsResubmit = isResubmit,
                HasNotificationsSet = preferences != null && preferences.NotificationTypes > NotificationTypes.None
            };

            return vm;
        }
    }
}