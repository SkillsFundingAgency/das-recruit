using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public class ManageNotificationsOrchestrator 
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        public ManageNotificationsOrchestrator(IRecruitVacancyClient recruitVacancyClient)
        {
            _recruitVacancyClient = recruitVacancyClient;
        }
        public async Task<ManageNotificationsViewModel> GetManageNotificationsViewModelAsync(VacancyUser vacancyUser)
        {
            var user = await _recruitVacancyClient.GetUsersDetailsAsync(vacancyUser.UserId);
            var userDetails = await _recruitVacancyClient.GetUserNotificationPreferencesAsync(user.Id);

            return userDetails == null ? new ManageNotificationsViewModel() : GetViewModelFromDomainModel(userDetails);
        }

        public async Task UpdateUserNotificationPreferencesAsync(ManageNotificationsEditModel editModel, VacancyUser vacancyUser)
        {
            var user = await _recruitVacancyClient.GetUsersDetailsAsync(vacancyUser.UserId);

            var preferences = GetDomainModel(editModel, user.Id);

            await _recruitVacancyClient.UpdateUserNotificationPreferencesAsync(preferences);
        }

        public ManageNotificationsAcknowledgementViewModel GetAcknowledgementViewModel(ManageNotificationsEditModel editModel, VacancyUser user)
        {
            return new ManageNotificationsAcknowledgementViewModel
            {
                IsApplicationSubmittedSelected = editModel.IsApplicationSubmittedSelected,
                IsVacancyClosingSoonSelected = editModel.IsVacancyClosingSoonSelected,
                IsVacancyRejectedSelected = editModel.IsVacancyRejectedSelected,
                IsUserSubmittedVacanciesSelected = editModel.NotificationScope.GetValueOrDefault() == NotificationScope.UserSubmittedVacancies,
                Frequency = editModel.NotificationFrequency.ToString().ToLower(),
                UserEmail = user.Email
            };
        }

        private UserNotificationPreferences GetDomainModel(ManageNotificationsEditModel sourceModel, Guid userId)
        {            
            var targetModel = new UserNotificationPreferences() { Id = userId };

            targetModel.NotificationFrequency = sourceModel.IsApplicationSubmittedSelected ? sourceModel.NotificationFrequency : null;

            if (sourceModel.NotificationScope.HasValue) targetModel.NotificationScope = sourceModel.NotificationScope.Value;

            targetModel.NotificationTypes = 
                (sourceModel.IsApplicationSubmittedSelected ? NotificationTypes.ApplicationSubmitted : NotificationTypes.None) 
                | (sourceModel.IsVacancyClosingSoonSelected ? NotificationTypes.VacancyClosingSoon : NotificationTypes.None) 
                | (sourceModel.IsVacancyRejectedSelected ? NotificationTypes.VacancyRejected : NotificationTypes.None);
                        
            return targetModel;
        }

        private ManageNotificationsViewModel GetViewModelFromDomainModel(UserNotificationPreferences preferences)
        {
            return new ManageNotificationsViewModel
            {
                IsVacancyRejectedSelected = (preferences.NotificationTypes & NotificationTypes.VacancyRejected) == NotificationTypes.VacancyRejected,
                IsVacancyClosingSoonSelected = (preferences.NotificationTypes & NotificationTypes.VacancyClosingSoon) == NotificationTypes.VacancyClosingSoon,
                IsApplicationSubmittedSelected = (preferences.NotificationTypes & NotificationTypes.ApplicationSubmitted) == NotificationTypes.ApplicationSubmitted,
                NotificationFrequency = preferences.NotificationFrequency,
                NotificationScope = preferences.NotificationScope
            };
        }
    }
}