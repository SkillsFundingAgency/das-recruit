﻿using Esfa.Recruit.Provider.Web.ViewModels;
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


        public SubmittedOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient)
        {
            _client = client;
            _vacancyClient = vacancyClient;
        }

        public async Task<VacancySubmittedConfirmationViewModel> GetVacancySubmittedConfirmationViewModelAsync(VacancyRouteModel vrm, VacancyUser vacancyUser)
        {
            var vacancy = await Utility.GetAuthorisedVacancyAsync(_client, _vacancyClient, vrm, RouteNames.Submitted_Index_Get);

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
                HasNotificationsSet = preferences != null && preferences.NotificationTypes > NotificationTypes.None
            };

            return vm;
        }
    }
}