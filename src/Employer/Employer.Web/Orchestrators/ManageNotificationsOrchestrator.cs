using System;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class ManageNotificationsOrchestrator(
    ILogger<ManageNotificationsOrchestrator> logger,
    IConfiguration configuration,
    IRecruitVacancyClient recruitVacancyClient,
    IMediator mediator)
    : EntityValidatingOrchestrator<UserNotificationPreferences, ManageNotificationsEditModel>(logger)
{
    private const string NotificationTypesIsRequiredForTheFirstTime = "Select when you want to receive emails about your adverts and applications";
    private readonly EntityValidationResult _notificationTypeIsRequiredForTheFirstTime = new EntityValidationResult
    {
        Errors = new[] 
        { 
            new EntityValidationError(1100, nameof(ManageNotificationsEditModel.HasAnySubscription), NotificationTypesIsRequiredForTheFirstTime, "1100") 
        }
    };

    public async Task<ManageNotificationsViewModelEx> NewGetManageNotificationsViewModelAsync(VacancyUser vacancyUser, string employerAccountId)
    {
        var result = await mediator.Send(new GetEmployerNotificationPreferencesQuery(vacancyUser.UserId));
        if (result == GetEmployerNotificationPreferencesQueryResult.None)
        {
            return null;
        }

        var applicationSubmittedPref = result.NotificationPreferences.GetForEvent(NotificationTypesEx.ApplicationSubmitted);
        var vacancyAppRefPref = result.NotificationPreferences.GetForEvent(NotificationTypesEx.VacancyApprovedOrRejected);
        return new ManageNotificationsViewModelEx
            {
                EmployerAccountId = employerAccountId,
                ApplicationSubmittedOptionValue = applicationSubmittedPref.Frequency == NotificationFrequencyEx.Never
                    ? nameof(NotificationFrequencyEx.Never)
                    : applicationSubmittedPref.Scope.ToString(),
                ApplicationSubmittedFrequencyMineOptionValue = applicationSubmittedPref.Frequency.ToString(),
                ApplicationSubmittedFrequencyAllOptionValue = applicationSubmittedPref.Frequency.ToString(),
                VacancyApprovedOrRejectedOptionValue = vacancyAppRefPref.Frequency == NotificationFrequencyEx.Never
                    ? nameof(NotificationFrequencyEx.Never)
                    : vacancyAppRefPref.Scope.ToString(),
            };
    }
        
    public async Task<OrchestratorResponse> NewUpdateUserNotificationPreferencesAsync(ManageNotificationsEditModelEx editModel, VacancyUser vacancyUser)
    {
        var currentPreferences = await mediator.Send(new GetEmployerNotificationPreferencesQuery(vacancyUser.UserId));
        if (currentPreferences == GetEmployerNotificationPreferencesQueryResult.None)
        {
            return new OrchestratorResponse(false);
        }

        var applicationSubmittedPref = currentPreferences.NotificationPreferences.GetForEvent(NotificationTypesEx.ApplicationSubmitted);
        var vacancyAppRefPref = currentPreferences.NotificationPreferences.GetForEvent(NotificationTypesEx.VacancyApprovedOrRejected);
        if (Enum.TryParse<NotificationScopeEx>(editModel.VacancyApprovedOrRejectedOptionValue, out var vacancyScope))
        {
            vacancyAppRefPref.Scope = vacancyScope;
            vacancyAppRefPref.Frequency = NotificationFrequencyEx.NotSet;
        }
        else
        {
            vacancyAppRefPref.Frequency = NotificationFrequencyEx.Never;
        }
            
        if (Enum.TryParse<NotificationScopeEx>(editModel.ApplicationSubmittedOptionValue, out var applicationScope))
        {
            applicationSubmittedPref.Scope = applicationScope;
            string frequencyValue = applicationSubmittedPref.Scope switch
            {
                NotificationScopeEx.UserSubmittedVacancies => editModel.ApplicationSubmittedFrequencyMineOptionValue,
                NotificationScopeEx.OrganisationVacancies => editModel.ApplicationSubmittedFrequencyAllOptionValue,
            };
            
            if (Enum.TryParse<NotificationFrequencyEx>(frequencyValue, out var applicationFrequency))
            {
                applicationSubmittedPref.Frequency = applicationFrequency;
            }
            else
            {
                return new OrchestratorResponse(new EntityValidationResult()
                {
                    Errors = [new EntityValidationError(
                        1100,
                        applicationSubmittedPref.Scope == NotificationScopeEx.OrganisationVacancies
                            ? nameof(ManageNotificationsEditModelEx.ApplicationSubmittedFrequencyAllOptionValue)
                            : nameof(ManageNotificationsEditModelEx.ApplicationSubmittedFrequencyMineOptionValue),
                        "Select how often you want to get emails about new applications",
                        "1100")]
                });
            }
        }
        else
        {
            applicationSubmittedPref.Frequency = NotificationFrequencyEx.Never;
        }

        await mediator.Send(new UpdateUserNotificationPreferencesCommand(currentPreferences.Id, currentPreferences.NotificationPreferences));
        return new OrchestratorResponse(true);
    }

    public async Task<ManageNotificationsViewModel> GetManageNotificationsViewModelAsync(VacancyUser vacancyUser, string employerAccountId)
    {
        var preferences = await recruitVacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);
        return GetViewModelFromDomainModel(preferences, employerAccountId);
    }

    public async Task<OrchestratorResponse> UpdateUserNotificationPreferencesAsync(ManageNotificationsEditModel editModel, VacancyUser vacancyUser)
    {
        var persistedPreferences = await recruitVacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);

        if (persistedPreferences.NotificationTypes == NotificationTypes.None && editModel.HasAnySubscription == false)
        {
            return new OrchestratorResponse(_notificationTypeIsRequiredForTheFirstTime);
        }

        var preferences = GetDomainModel(editModel, vacancyUser.UserId);

        return await ValidateAndExecute(
            preferences,
            v => recruitVacancyClient.ValidateUserNotificationPreferences(preferences),
            v => recruitVacancyClient.UpdateUserNotificationPreferencesAsync(preferences)
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
            EnvironmentIsProd = configuration["Environment"].Equals("Prod", StringComparison.CurrentCultureIgnoreCase)
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