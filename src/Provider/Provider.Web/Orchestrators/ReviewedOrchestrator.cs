using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Reviewed;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ReviewedOrchestrator
    {
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;


        public ReviewedOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<VacancyReviewedConfirmationViewModel> GetVacancyReviewedOrchestratorConfirmationViewModelAsync(VacancyRouteModel vrm, VacancyUser vacancyUser)
        {
            var vacancy = await Utility.GetAuthorisedVacancyAsync(_client, _vacancyClient, vrm, RouteNames.Reviewed_Index_Get);
            var employer = await _client.GetProviderEmployerVacancyDataAsync(vrm.Ukprn, vacancy.EmployerAccountId);
            var preferences = await _vacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);

            if (vacancy.Status != VacancyStatus.Review)
                throw new InvalidStateException(string.Format(ErrorMessages.VacancyNotReviewedSuccessfully, vacancy.Title));

            var vm = new VacancyReviewedConfirmationViewModel
            {
                Title = vacancy.Title,
                VacancyReference = vacancy.VacancyReference?.ToString(),
                EmployerName = employer.Name,
                IsResubmit = vacancy.ReviewDate.HasValue,
                IsVacancyRejectedByEmployerNotificationSelected = preferences.NotificationTypes.HasFlag(NotificationTypes.VacancyRejectedByEmployer)
            };

            return vm;
        }
    }
}