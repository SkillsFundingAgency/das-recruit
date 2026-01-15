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

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class ManageNotificationsOrchestrator(
    ILogger<ManageNotificationsOrchestrator> logger,
    IRecruitVacancyClient recruitVacancyClient,
    IMediator mediator)
    : EntityValidatingOrchestrator<UserNotificationPreferences, ManageNotificationsEditModel>(logger)
{
    private const string NotificationTypesIsRequiredForTheFirstTime = "Select when you want to receive emails about your adverts and applications";
    private readonly EntityValidationResult _notificationTypeIsRequiredForTheFirstTime = new()
    {
        Errors =
        [
            new EntityValidationError(1100, nameof(ManageNotificationsEditModel.HasAnySubscription), NotificationTypesIsRequiredForTheFirstTime, "1100")
        ]
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
                ApplicationSubmittedValue = applicationSubmittedPref.Frequency == NotificationFrequencyEx.Never
                    ? nameof(NotificationFrequencyEx.Never)
                    : applicationSubmittedPref.Scope.ToString(),
                ApplicationSubmittedFrequencyValue = applicationSubmittedPref.Frequency.ToString(),
                VacancyApprovedOrRejectedValue = vacancyAppRefPref.Frequency == NotificationFrequencyEx.Never
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
        if (Enum.TryParse<NotificationScopeEx>(editModel.VacancyApprovedOrRejectedValue, out var vacancyScope))
        {
            vacancyAppRefPref.Scope = vacancyScope;
            vacancyAppRefPref.Frequency = NotificationFrequencyEx.NotSet;
        }
        else
        {
            vacancyAppRefPref.Frequency = NotificationFrequencyEx.Never;
        }
            
        if (Enum.TryParse<NotificationScopeEx>(editModel.ApplicationSubmittedValue, out var applicationScope))
        {
            applicationSubmittedPref.Scope = applicationScope;
            
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
        else
        {
            applicationSubmittedPref.Frequency = NotificationFrequencyEx.Never;
        }

        await mediator.Send(new UpdateUserNotificationPreferencesCommand(currentPreferences.Id, currentPreferences.NotificationPreferences));
        return new OrchestratorResponse(true);
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
            _ => recruitVacancyClient.ValidateUserNotificationPreferences(preferences),
            _ => recruitVacancyClient.UpdateUserNotificationPreferencesAsync(preferences)
        );
    }

    public Task UnsubscribeUserNotificationsAsync(VacancyUser vacancyUser)
    {
        return UpdateUserNotificationPreferencesAsync(new ManageNotificationsEditModel(), vacancyUser);
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

    protected override EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel> DefineMappings()
    {
        var mappings = new EntityToViewModelPropertyMappings<UserNotificationPreferences, ManageNotificationsEditModel>();
        mappings.Add(n => n.NotificationScope, m => m.NotificationScope);
        mappings.Add(n => n.NotificationFrequency, m => m.NotificationFrequency);
        return mappings;
    }
}