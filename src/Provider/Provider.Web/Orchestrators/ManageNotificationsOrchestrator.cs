using System;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class ManageNotificationsOrchestrator(
    ILogger<ManageNotificationsOrchestrator> logger,
    IMediator mediator)
    : EntityValidatingOrchestrator<UserNotificationPreferences, ManageNotificationsEditModel>(logger)
{
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
                return new OrchestratorResponse(new EntityValidationResult
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
            preference.Frequency = NotificationFrequencyEx.Immediately;
        }
        else
        {
            preference.Frequency = NotificationFrequencyEx.Never;
        }
    }

    protected override EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel>
        {
            {n => n.NotificationScope, m => m.NotificationScope},
            {n => n.NotificationFrequency, m => m.NotificationFrequency}
        };
        return mappings;
    }
}