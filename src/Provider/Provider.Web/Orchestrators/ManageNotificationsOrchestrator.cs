using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ManageNotificationsOrchestrator : EntityValidatingOrchestrator<UserNotificationPreferences, ManageNotificationsEditModel>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;

        private const string NotificationTypesIsRequiredForTheFirstTime = "Choose when you’d like to receive emails";
        private readonly EntityValidationResult _notificationTypeIsRequiredForTheFirstTime = new EntityValidationResult
        {
            Errors = new[] 
            { 
                new EntityValidationError(1100, nameof(ManageNotificationsEditModel.HasAnySubscription), NotificationTypesIsRequiredForTheFirstTime, "1100") 
            }
        };

        public ManageNotificationsOrchestrator(
            ILogger<ManageNotificationsOrchestrator> logger, 
            IRecruitVacancyClient recruitVacancyClient) : base(logger)
        {
            _recruitVacancyClient = recruitVacancyClient;
        }
        public async Task<ManageNotificationsViewModel> GetManageNotificationsViewModelAsync(VacancyUser vacancyUser)
        {
            var preferences = await _recruitVacancyClient.GetUserNotificationPreferencesByDfEUserIdAsync(vacancyUser.UserId, vacancyUser.DfEUserId);

            if (string.IsNullOrEmpty(preferences.DfeUserId))
            {
                preferences.DfeUserId = vacancyUser.DfEUserId;
                await _recruitVacancyClient.UpdateUserNotificationPreferencesAsync(preferences);
            }
            
            return GetViewModelFromDomainModel(preferences);
        }

        public async Task<OrchestratorResponse> UpdateUserNotificationPreferencesAsync(ManageNotificationsEditModel editModel, VacancyUser vacancyUser)
        {
            var persistedPreferences =
                await _recruitVacancyClient.GetUserNotificationPreferencesByDfEUserIdAsync(vacancyUser.UserId,
                    vacancyUser.DfEUserId)
                ?? await _recruitVacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);

            if (persistedPreferences.NotificationTypes == NotificationTypes.None && editModel.HasAnySubscription == false)
            {
                return new OrchestratorResponse(_notificationTypeIsRequiredForTheFirstTime);
            }

            var preferences = GetDomainModel(editModel, vacancyUser.UserId, vacancyUser.DfEUserId);

            return await ValidateAndExecute(
                preferences,
                v => _recruitVacancyClient.ValidateUserNotificationPreferences(preferences),
                v => _recruitVacancyClient.UpdateUserNotificationPreferencesAsync(preferences)
            );
        }

        public Task UnsubscribeUserNotificationsAsync(VacancyUser vacancyUser)
        {
            return UpdateUserNotificationPreferencesAsync(new ManageNotificationsEditModel(), vacancyUser);
        }

        public ManageNotificationsAcknowledgementViewModel GetAcknowledgementViewModel(ManageNotificationsEditModel editModel, VacancyUser user)
        {
            return new ManageNotificationsAcknowledgementViewModel
            {
                IsApplicationSubmittedSelected = editModel.IsApplicationSubmittedSelected,
                IsVacancyRejectedByEmployerSelected = editModel.IsVacancyRejectedByEmployerSelected,
                IsVacancyClosingSoonSelected = editModel.IsVacancyClosingSoonSelected,
                IsVacancyRejectedSelected = editModel.IsVacancyRejectedSelected,
                IsUserSubmittedVacanciesSelected = editModel.NotificationScope.GetValueOrDefault() == NotificationScope.UserSubmittedVacancies,
                Frequency = editModel.NotificationFrequency.ToString().ToLower(),
                UserEmail = user.Email
            };
        }

        private UserNotificationPreferences GetDomainModel(ManageNotificationsEditModel sourceModel, string idamsUserId, string dfeUserId)
        {            
            var targetModel = new UserNotificationPreferences() { Id = idamsUserId, DfeUserId = dfeUserId};
            if (!sourceModel.HasAnySubscription) return targetModel;

            targetModel.NotificationFrequency = sourceModel.IsApplicationSubmittedSelected ? sourceModel.NotificationFrequency : null;

            if (sourceModel.NotificationScope.HasValue) targetModel.NotificationScope = sourceModel.NotificationScope.Value;

            targetModel.NotificationTypes = 
                (sourceModel.IsApplicationSubmittedSelected ? NotificationTypes.ApplicationSubmitted : NotificationTypes.None) 
                | (sourceModel.IsVacancyClosingSoonSelected ? NotificationTypes.VacancyClosingSoon : NotificationTypes.None) 
                | (sourceModel.IsVacancyRejectedSelected ? NotificationTypes.VacancyRejected : NotificationTypes.None)
                | (sourceModel.IsVacancyRejectedByEmployerSelected ? NotificationTypes.VacancyRejectedByEmployer : NotificationTypes.None);
                        
            return targetModel;
        }

        private ManageNotificationsViewModel GetViewModelFromDomainModel(UserNotificationPreferences preferences)
        {
            return new ManageNotificationsViewModel
            {
                IsVacancyRejectedSelected = (preferences.NotificationTypes & NotificationTypes.VacancyRejected) == NotificationTypes.VacancyRejected,
                IsVacancyClosingSoonSelected = (preferences.NotificationTypes & NotificationTypes.VacancyClosingSoon) == NotificationTypes.VacancyClosingSoon,
                IsApplicationSubmittedSelected = (preferences.NotificationTypes & NotificationTypes.ApplicationSubmitted) == NotificationTypes.ApplicationSubmitted,
                IsVacancyRejectedByEmployerSelected = (preferences.NotificationTypes & NotificationTypes.VacancyRejectedByEmployer) == NotificationTypes.VacancyRejectedByEmployer,
                NotificationFrequency = preferences.NotificationFrequency,
                NotificationScope = preferences.NotificationScope
            };
        }

        protected override EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel>();
            mappings.Add(n => n.NotificationScope, m => m.NotificationScope);
            mappings.Add(n => n.NotificationFrequency, m => m.NotificationFrequency);
            return mappings;
        }
    }
}