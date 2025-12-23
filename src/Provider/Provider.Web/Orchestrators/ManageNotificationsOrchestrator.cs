using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class ManageNotificationsOrchestrator(
    ILogger<ManageNotificationsOrchestrator> logger,
    IRecruitVacancyClient recruitVacancyClient,
    IMediator mediator)
    : EntityValidatingOrchestrator<UserNotificationPreferences, ManageNotificationsEditModel>(logger)
{
    private const string NotificationTypesIsRequiredForTheFirstTime = "Choose when you’d like to receive emails";
    private readonly EntityValidationResult _notificationTypeIsRequiredForTheFirstTime = new EntityValidationResult
    {
        Errors =
        [
            new EntityValidationError(1100, nameof(ManageNotificationsEditModel.HasAnySubscription), NotificationTypesIsRequiredForTheFirstTime, "1100")
        ]
    };

    public async Task<OrchestratorResponse> UpdateUserNotificationPreferencesAsync(ManageNotificationsEditModel editModel, VacancyUser vacancyUser)
    {
        var persistedPreferences =
            await recruitVacancyClient.GetUserNotificationPreferencesByDfEUserIdAsync(vacancyUser.UserId,
                vacancyUser.DfEUserId)
            ?? await recruitVacancyClient.GetUserNotificationPreferencesAsync(vacancyUser.UserId);

        if (persistedPreferences.NotificationTypes == NotificationTypes.None && editModel.HasAnySubscription == false)
        {
            return new OrchestratorResponse(_notificationTypeIsRequiredForTheFirstTime);
        }

        var preferences = GetDomainModel(editModel, vacancyUser.UserId, vacancyUser.DfEUserId);

        return await ValidateAndExecute(
            preferences,
            _ => recruitVacancyClient.ValidateUserNotificationPreferences(preferences),
            _ => recruitVacancyClient.UpdateUserNotificationPreferencesAsync(preferences)
        );
    }

    public async Task<ManageNotificationsViewModelEx> NewGetManageNotificationsViewModelAsync(VacancyUser vacancyUser)
    {
        var result = await mediator.Send(new GetProviderNotificationPreferencesQuery(vacancyUser.DfEUserId));
        if (result == GetProviderNotificationPreferencesQueryResult.None)
        {
            return null;
        }

        var applicationSubmittedPref = result.NotificationPreferences.GetForEvent(NotificationTypesEx.ApplicationSubmitted);
        var vacancyAppRefPref = result.NotificationPreferences.GetForEvent(NotificationTypesEx.VacancyApprovedOrRejected);
        var sharedAppReviewPref = result.NotificationPreferences.GetForEvent(NotificationTypesEx.SharedApplicationReviewedByEmployer);
        var providerAttachedPref = result.NotificationPreferences.GetForEvent(NotificationTypesEx.ProviderAttachedToVacancy);
        
        return new ManageNotificationsViewModelEx
        {
            ApplicationSubmittedValue = applicationSubmittedPref.Frequency == NotificationFrequencyEx.Never
                ? nameof(NotificationFrequencyEx.Never)
                : applicationSubmittedPref.Scope.ToString(),
            ApplicationSubmittedFrequencyValue = applicationSubmittedPref.Frequency.ToString(),
            VacancyApprovedOrRejectedValue = vacancyAppRefPref.Frequency == NotificationFrequencyEx.Never
                ? nameof(NotificationFrequencyEx.Never)
                : vacancyAppRefPref.Scope.ToString(),
            SharedApplicationReviewedValue = sharedAppReviewPref.Frequency == NotificationFrequencyEx.Never
                ? nameof(NotificationFrequencyEx.Never)
                : sharedAppReviewPref.Scope.ToString(),
            ProviderAttachedToVacancyValue = providerAttachedPref.Frequency == NotificationFrequencyEx.Never
                ? nameof(NotificationFrequencyEx.Never)
                : nameof(NotificationFrequencyEx.Immediately),
        };
    }

    public async Task<OrchestratorResponse> NewUpdateUserNotificationPreferencesAsync(ManageNotificationsEditModelEx editModel, VacancyUser vacancyUser)
    {
        var currentPreferences = await mediator.Send(new GetProviderNotificationPreferencesQuery(vacancyUser.DfEUserId));
        if (currentPreferences == GetProviderNotificationPreferencesQueryResult.None)
        {
            return new OrchestratorResponse(false);
        }

        var applicationSubmittedPref = currentPreferences.NotificationPreferences.GetForEvent(NotificationTypesEx.ApplicationSubmitted);
        var vacancyAppRefPref = currentPreferences.NotificationPreferences.GetForEvent(NotificationTypesEx.VacancyApprovedOrRejected);
        var reviewedSharedAppPref = currentPreferences.NotificationPreferences.GetForEvent(NotificationTypesEx.SharedApplicationReviewedByEmployer);
        var providerAttachedPref = currentPreferences.NotificationPreferences.GetForEvent(NotificationTypesEx.ProviderAttachedToVacancy);

        if (Enum.TryParse<NotificationFrequencyEx>(editModel.ProviderAttachedToVacancyValue, out var providerAttachedFrequency))
        {
            providerAttachedPref.Frequency = providerAttachedFrequency;
            providerAttachedPref.Scope = NotificationScopeEx.NotSet;
        }
        
        ParseScope(editModel.VacancyApprovedOrRejectedValue, vacancyAppRefPref);
        ParseScope(editModel.SharedApplicationReviewedValue, reviewedSharedAppPref);
        ParseScope(editModel.ApplicationSubmittedValue, applicationSubmittedPref);

        if (applicationSubmittedPref.Frequency != NotificationFrequencyEx.Never)
        {
            if (Enum.TryParse<NotificationFrequencyEx>(editModel.ApplicationSubmittedFrequencyValue, out var applicationFrequency))
            {
                applicationSubmittedPref.Frequency = applicationFrequency;
            }
            else
            {
                return new OrchestratorResponse(new EntityValidationResult()
                {
                    Errors = [new EntityValidationError(
                        1100,
                        nameof(ManageNotificationsEditModelEx.ApplicationSubmittedFrequencyValue),
                        "Select how often you want to get emails about new applications",
                        "1100")]
                });
            }
        }

        await mediator.Send(new UpdateUserNotificationPreferencesCommand(currentPreferences.Id, currentPreferences.NotificationPreferences));
        return new OrchestratorResponse(true);
    }

    private static void ParseScope(string scopeText, NotificationPreference preference)
    {
        if (Enum.TryParse<NotificationScopeEx>(scopeText, out var scope))
        {
            preference.Scope = scope;
            preference.Frequency = NotificationFrequencyEx.NotSet;
        }
        else
        {
            preference.Frequency = NotificationFrequencyEx.Never;
        }
    }

    public Task UnsubscribeUserNotificationsAsync(VacancyUser vacancyUser)
    {
        return UpdateUserNotificationPreferencesAsync(new ManageNotificationsEditModel(), vacancyUser);
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

    protected override EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel>();
        mappings.Add(n => n.NotificationScope, m => m.NotificationScope);
        mappings.Add(n => n.NotificationFrequency, m => m.NotificationFrequency);
        return mappings;
    }
}