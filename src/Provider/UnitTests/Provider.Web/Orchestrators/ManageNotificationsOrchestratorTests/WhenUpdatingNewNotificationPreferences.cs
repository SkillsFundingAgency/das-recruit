using System.Linq;
using System.Threading;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.ManageNotificationsOrchestratorTests;

public class WhenUpdatingNewNotificationPreferences
{
    [Test, MoqAutoData]
    public async Task It_Fails_When_User_Not_Found(
        VacancyUser user,
        ManageNotificationsEditModelEx editModel,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        GetProviderNotificationPreferencesQuery? capturedQuery = null;
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest<GetProviderNotificationPreferencesQueryResult> x, CancellationToken _) => capturedQuery = x as GetProviderNotificationPreferencesQuery)
            .ReturnsAsync(GetProviderNotificationPreferencesQueryResult.None);

        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeFalse();
        capturedQuery.Should().NotBeNull();
        capturedQuery.DfEUserId.Should().Be(user.DfEUserId);
    }
    
    [Test, MoqAutoData]
    public async Task Preferences_Can_Be_Set_To_Never(
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        var response = new GetProviderNotificationPreferencesQueryResult
        {
            Id = Guid.NewGuid(),
            DfEUserId = user.DfEUserId,
            NotificationPreferences = new NotificationPreferences
            {
                EventPreferences = [
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Immediately },
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        UpdateUserNotificationPreferencesCommand? capturedCommand = null;
        mediator
            .Setup(x => x.Send(It.IsAny<UpdateUserNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest x, CancellationToken _) => capturedCommand = x as UpdateUserNotificationPreferencesCommand);

        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedOptionValue = nameof(NotificationFrequencyEx.Never),
            VacancyApprovedOrRejectedOptionValue = nameof(NotificationFrequencyEx.Never),
            SharedApplicationReviewedOptionValue = nameof(NotificationFrequencyEx.Never),
            ProviderAttachedToVacancyOptionValue = nameof(NotificationFrequencyEx.Never),
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeTrue();
        capturedCommand.Should().NotBeNull();
        capturedCommand.Id.Should().Be(response.Id);
        capturedCommand.NotificationPreferences.EventPreferences.Should().AllSatisfy(x => x.Frequency.Should().Be(NotificationFrequencyEx.Never));
    }
    
    [Test, MoqAutoData]
    public async Task Preferences_Can_Be_Set_To_OrganisationVacancies(
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        var response = new GetProviderNotificationPreferencesQueryResult
        {
            Id = Guid.NewGuid(),
            DfEUserId = user.DfEUserId,
            NotificationPreferences = new NotificationPreferences
            {
                EventPreferences = [
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Immediately },
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        UpdateUserNotificationPreferencesCommand? capturedCommand = null;
        mediator
            .Setup(x => x.Send(It.IsAny<UpdateUserNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest x, CancellationToken _) => capturedCommand = x as UpdateUserNotificationPreferencesCommand);

        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedOptionValue = nameof(NotificationScopeEx.OrganisationVacancies),
            VacancyApprovedOrRejectedOptionValue = nameof(NotificationScopeEx.OrganisationVacancies),
            SharedApplicationReviewedOptionValue = nameof(NotificationScopeEx.OrganisationVacancies),
            ApplicationSubmittedFrequencyAllOptionValue = nameof(NotificationFrequencyEx.Daily),
            ApplicationSubmittedFrequencyMineOptionValue = nameof(NotificationFrequencyEx.Daily),
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeTrue();
        capturedCommand.Should().NotBeNull();
        capturedCommand.Id.Should().Be(response.Id);
        capturedCommand.NotificationPreferences.EventPreferences.Should().AllSatisfy(x => x.Scope.Should().Be(NotificationScopeEx.OrganisationVacancies));
    }
    
    [Test, MoqAutoData]
    public async Task Preferences_Can_Be_Set_To_UserSubmittedVacancies(
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        var response = new GetProviderNotificationPreferencesQueryResult
        {
            Id = Guid.NewGuid(),
            DfEUserId = user.DfEUserId,
            NotificationPreferences = new NotificationPreferences
            {
                EventPreferences = [
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Immediately },
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        UpdateUserNotificationPreferencesCommand? capturedCommand = null;
        mediator
            .Setup(x => x.Send(It.IsAny<UpdateUserNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest x, CancellationToken _) => capturedCommand = x as UpdateUserNotificationPreferencesCommand);

        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedOptionValue = nameof(NotificationScopeEx.UserSubmittedVacancies),
            VacancyApprovedOrRejectedOptionValue = nameof(NotificationScopeEx.UserSubmittedVacancies),
            SharedApplicationReviewedOptionValue = nameof(NotificationScopeEx.UserSubmittedVacancies),
            ApplicationSubmittedFrequencyAllOptionValue = nameof(NotificationFrequencyEx.Daily),
            ApplicationSubmittedFrequencyMineOptionValue = nameof(NotificationFrequencyEx.Daily),
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeTrue();
        capturedCommand.Should().NotBeNull();
        capturedCommand.Id.Should().Be(response.Id);
        capturedCommand.NotificationPreferences.EventPreferences
            .Where(x => x.Event != NotificationTypesEx.ProviderAttachedToVacancy)
            .Should().AllSatisfy(x => x.Scope.Should().Be(NotificationScopeEx.UserSubmittedVacancies));
    }
    
    [Test, MoqAutoData]
    public async Task Frequency_Must_Be_Selected(
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        var response = new GetProviderNotificationPreferencesQueryResult
        {
            Id = Guid.NewGuid(),
            DfEUserId = user.DfEUserId,
            NotificationPreferences = new NotificationPreferences
            {
                EventPreferences = [
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Immediately },
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedOptionValue = nameof(NotificationScopeEx.UserSubmittedVacancies),
            VacancyApprovedOrRejectedOptionValue = nameof(NotificationScopeEx.UserSubmittedVacancies),
            SharedApplicationReviewedOptionValue = nameof(NotificationScopeEx.UserSubmittedVacancies),
            ApplicationSubmittedFrequencyAllOptionValue = null,
            ApplicationSubmittedFrequencyMineOptionValue = null,
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeFalse();
        result.Errors.Errors.Should().HaveCount(1);
    }
}