using System.Threading;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetProviderNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.ManageNotificationsOrchestratorTests;

public class WhenGettingNewNotificationPreferencesViewModel
{
    [Test, MoqAutoData]
    public async Task Returns_Null(
        VacancyUser user,
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
        var result = await sut.NewGetManageNotificationsViewModelAsync(user);

        // assert
        result.Should().BeNull();
        capturedQuery.Should().NotBeNull();
        capturedQuery.DfEUserId.Should().Be(user.DfEUserId);
    }
    
    [Test]
    [MoqInlineAutoData(NotificationScopeEx.NotSet, NotificationFrequencyEx.Never, nameof(NotificationFrequencyEx.Never), nameof(NotificationFrequencyEx.Never))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Weekly))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Daily))]
    public async Task Application_Notifications_Should_Be(
        NotificationScopeEx scope,
        NotificationFrequencyEx frequency,
        string expectedOptionValue,
        string expectedFrequencyValue,
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
                    new NotificationPreference
                    {
                        Event = NotificationTypesEx.ApplicationSubmitted,
                        Scope = scope,
                        Frequency = frequency
                    },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                ]
            }
        };
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    
        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user);
    
        // assert
        result.Should().NotBeNull();
        result.ApplicationSubmittedValue.Should().Be(expectedOptionValue);
        result.ApplicationSubmittedFrequencyValue.Should().Be(expectedFrequencyValue);
    }
    
    [Test]
    [MoqInlineAutoData(NotificationScopeEx.NotSet, NotificationFrequencyEx.Never, nameof(NotificationFrequencyEx.Never))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.UserSubmittedVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.OrganisationVacancies))]
    public async Task VacancyApprovedOrRejected_Notifications_Should_Be(
        NotificationScopeEx scope,
        NotificationFrequencyEx frequency,
        string expectedOptionValue,
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
                    new NotificationPreference
                    {
                        Event = NotificationTypesEx.VacancyApprovedOrRejected,
                        Scope = scope,
                        Frequency = frequency,
                    },
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                ]
            }
        };
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    
        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user);
    
        // assert
        result.Should().NotBeNull();
        result.VacancyApprovedOrRejectedValue.Should().Be(expectedOptionValue);
    }
    
    [Test]
    [MoqInlineAutoData(NotificationFrequencyEx.Never, nameof(NotificationFrequencyEx.Never))]
    [MoqInlineAutoData(NotificationFrequencyEx.Immediately, nameof(NotificationFrequencyEx.Immediately))]
    public async Task ProviderAttachedToVacancy_Notifications_Should_Be(
        NotificationFrequencyEx frequency,
        string expectedOptionValue,
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
                    new NotificationPreference
                    {
                        Event = NotificationTypesEx.ProviderAttachedToVacancy,
                        Scope = NotificationScopeEx.NotSet,
                        Frequency = frequency,
                    },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.SharedApplicationReviewedByEmployer, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                ]
            }
        };
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    
        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user);
    
        // assert
        result.Should().NotBeNull();
        result.ProviderAttachedToVacancyValue.Should().Be(expectedOptionValue);
    }
    
    [Test]
    [MoqInlineAutoData(NotificationScopeEx.NotSet, NotificationFrequencyEx.Never, nameof(NotificationFrequencyEx.Never))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.UserSubmittedVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.OrganisationVacancies))]
    public async Task SharedApplicationReviewedByEmployer_Notifications_Should_Be(
        NotificationScopeEx scope,
        NotificationFrequencyEx frequency,
        string expectedOptionValue,
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
                    new NotificationPreference
                    {
                        Event = NotificationTypesEx.SharedApplicationReviewedByEmployer,
                        Scope = scope,
                        Frequency = frequency,
                    },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                    new NotificationPreference { Event = NotificationTypesEx.ProviderAttachedToVacancy, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never },
                ]
            }
        };
        mediator
            .Setup(x => x.Send(It.IsAny<GetProviderNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    
        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user);
    
        // assert
        result.Should().NotBeNull();
        result.SharedApplicationReviewedValue.Should().Be(expectedOptionValue);
    }
}