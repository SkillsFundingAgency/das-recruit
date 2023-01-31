using System;
using System.Threading.Tasks;
using Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Employer.Web.Orchestrators 
{
    public class ManageNotificationsOrchestrator : EntityValidatingOrchestrator<UserNotificationPreferences, ManageNotificationsEditModel>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly RecruitConfiguration _recruitConfiguration;
        private IConfiguration _configuration;
        
        private const string NotificationTypesIsRequiredForTheFirstTime = "Select when you want to receive emails about your adverts and applications";
        private readonly EntityValidationResult _notificationTypeIsRequiredForTheFirstTime = new EntityValidationResult
        {
            Errors = new[] 
            { 
                new EntityValidationError(1100, nameof(ManageNotificationsEditModel.HasAnySubscription), NotificationTypesIsRequiredForTheFirstTime, "1100") 
            }
        };

        public ManageNotificationsOrchestrator(
            ILogger<ManageNotificationsOrchestrator> logger,
            RecruitConfiguration recruitConfiguration,
            IConfiguration configuration,
            IRecruitVacancyClient recruitVacancyClient) : base(logger)
        {
            _recruitVacancyClient = recruitVacancyClient;
            _configuration = configuration;
            _recruitConfiguration = recruitConfiguration;
        }


        public async Task<ManageNotificationsViewModel> GetManageNotificationsViewModelAsync(VacancyUser vacancyUser, string employerAccountId)
        {
            var preferences = await _recruitVacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);
            

            return GetViewModelFromDomainModel(preferences, employerAccountId);
        }

        public async Task<OrchestratorResponse> UpdateUserNotificationPreferencesAsync(ManageNotificationsEditModel editModel, VacancyUser vacancyUser)
        {
            var persistedPreferences = await _recruitVacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);

            if (persistedPreferences.NotificationTypes == NotificationTypes.None && editModel.HasAnySubscription == false)
            {
                return new OrchestratorResponse(_notificationTypeIsRequiredForTheFirstTime);
            }

            var preferences = GetDomainModel(editModel, vacancyUser.UserId);

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
                EmployerAccountId = editModel.EmployerAccountId,
                IsApplicationSubmittedSelected = editModel.IsApplicationSubmittedSelected,
                IsVacancySentForEmployerReviewSelected = editModel.IsVacancySentForEmployerReviewSelected,
                IsVacancyClosingSoonSelected = editModel.IsVacancyClosingSoonSelected,
                IsVacancyRejectedSelected = editModel.IsVacancyRejectedSelected,
                IsUserSubmittedVacanciesSelected = editModel.NotificationScope.GetValueOrDefault() == NotificationScope.UserSubmittedVacancies,
                Frequency = editModel.NotificationFrequency.ToString().ToLower(),
                UserEmail = user.Email
            };
        }

        private UserNotificationPreferences GetDomainModel(ManageNotificationsEditModel sourceModel, string idamsUserId)
        {            
            var targetModel = new UserNotificationPreferences() { Id = idamsUserId };
            if (!sourceModel.HasAnySubscription) return targetModel;

            targetModel.NotificationFrequency = sourceModel.IsApplicationSubmittedSelected ? sourceModel.NotificationFrequency : null;

            if (sourceModel.NotificationScope.HasValue) targetModel.NotificationScope = sourceModel.NotificationScope.Value;

            targetModel.NotificationTypes = 
                (sourceModel.IsApplicationSubmittedSelected ? NotificationTypes.ApplicationSubmitted : NotificationTypes.None) 
                | (sourceModel.IsVacancyClosingSoonSelected ? NotificationTypes.VacancyClosingSoon : NotificationTypes.None) 
                | (sourceModel.IsVacancyRejectedSelected ? NotificationTypes.VacancyRejected : NotificationTypes.None)
                | (sourceModel.IsVacancySentForEmployerReviewSelected ? NotificationTypes.VacancySentForReview : NotificationTypes.None);
                        
            return targetModel;
        }

        private ManageNotificationsViewModel GetViewModelFromDomainModel(UserNotificationPreferences preferences, string employerAccountId)
        {

            return new ManageNotificationsViewModel
            {
                EmployerAccountId = employerAccountId,
                IsVacancyRejectedSelected = (preferences.NotificationTypes & NotificationTypes.VacancyRejected) ==
                                            NotificationTypes.VacancyRejected,
                IsVacancyClosingSoonSelected = (preferences.NotificationTypes & NotificationTypes.VacancyClosingSoon) ==
                                               NotificationTypes.VacancyClosingSoon,
                IsApplicationSubmittedSelected =
                    (preferences.NotificationTypes & NotificationTypes.ApplicationSubmitted) ==
                    NotificationTypes.ApplicationSubmitted,
                IsVacancySentForEmployerReviewSelected =
                    (preferences.NotificationTypes & NotificationTypes.VacancySentForReview) ==
                    NotificationTypes.VacancySentForReview,
                NotificationFrequency = preferences.NotificationFrequency,
                NotificationScope = preferences.NotificationScope,
                UseGovSignIn = _recruitConfiguration.UseGovSignIn,
                EnvironmentIsProd = _configuration["EnvironmentName"].Equals("Prod", StringComparison.CurrentCultureIgnoreCase)
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