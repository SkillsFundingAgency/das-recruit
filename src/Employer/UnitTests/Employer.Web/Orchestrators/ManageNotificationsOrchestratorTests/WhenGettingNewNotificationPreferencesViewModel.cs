using System.Threading;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.ManageNotificationsOrchestratorTests;

public class WhenGettingNewNotificationPreferencesViewModel
{
    [Test, MoqAutoData]
    public async Task Returns_Null(
        string employerAccountId,
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        GetEmployerNotificationPreferencesQuery? capturedQuery = null;
        mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest<GetEmployerNotificationPreferencesQueryResult> x, CancellationToken _) => capturedQuery = x as GetEmployerNotificationPreferencesQuery)
            .ReturnsAsync(GetEmployerNotificationPreferencesQueryResult.None);

        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user, employerAccountId);

        // assert
        result.Should().BeNull();
        capturedQuery.Should().NotBeNull();
        capturedQuery.IdamsId.Should().Be(user.UserId);
    }
    
    [Test]
    [MoqInlineAutoData(NotificationScopeEx.Default, NotificationFrequencyEx.Never, nameof(NotificationFrequencyEx.Never), nameof(NotificationFrequencyEx.Never))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Weekly))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Daily))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Immediately, nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Immediately))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Weekly))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Daily))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Immediately, nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Immediately))]
    public async Task Application_Notifications_Should_Be(
        NotificationScopeEx scope,
        NotificationFrequencyEx frequency,
        string expectedOptionValue,
        string expectedFrequencyValue,
        string employerAccountId,
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        var response = new GetEmployerNotificationPreferencesQueryResult
        {
            Id = Guid.NewGuid(),
            IdamsId = user.UserId,
            NotificationPreferences = new NotificationPreferences
            {
                EventPreferences = [
                    new NotificationPreference
                    {
                        Event = NotificationTypesEx.ApplicationSubmitted,
                        Scope = scope,
                        Frequency = frequency
                    },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejectedByDfE, Scope = NotificationScopeEx.Default, Frequency = NotificationFrequencyEx.Never }
                ]
            }
        };
        mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user, employerAccountId);

        // assert
        result.Should().NotBeNull();
        result.ApplicationSubmittedOptionValue.Should().Be(expectedOptionValue);
        result.ApplicationSubmittedFrequencyAllOptionValue.Should().Be(expectedFrequencyValue);
        result.ApplicationSubmittedFrequencyMineOptionValue.Should().Be(expectedFrequencyValue);
    }
    
    [Test]
    [MoqInlineAutoData(NotificationScopeEx.Default, NotificationFrequencyEx.Never, nameof(NotificationFrequencyEx.Never))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.UserSubmittedVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.UserSubmittedVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Immediately, nameof(NotificationScopeEx.UserSubmittedVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Weekly, nameof(NotificationScopeEx.OrganisationVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Daily, nameof(NotificationScopeEx.OrganisationVacancies))]
    [MoqInlineAutoData(NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Immediately, nameof(NotificationScopeEx.OrganisationVacancies))]
    public async Task VacancyApprovedOrRejected_Notifications_Should_Be(
        NotificationScopeEx scope,
        NotificationFrequencyEx frequency,
        string expectedOptionValue,
        string employerAccountId,
        VacancyUser user,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] ManageNotificationsOrchestrator sut)
    {
        // arrange
        var response = new GetEmployerNotificationPreferencesQueryResult
        {
            Id = Guid.NewGuid(),
            IdamsId = user.UserId,
            NotificationPreferences = new NotificationPreferences
            {
                EventPreferences = [
                    new NotificationPreference
                    {
                        Event = NotificationTypesEx.VacancyApprovedOrRejectedByDfE,
                        Scope = scope,
                        Frequency = frequency,
                    },
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.Default, Frequency = NotificationFrequencyEx.Never },
                ]
            }
        };
        mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // act
        var result = await sut.NewGetManageNotificationsViewModelAsync(user, employerAccountId);

        // assert
        result.Should().NotBeNull();
        result.VacancyApprovedOrRejectedOptionValue.Should().Be(expectedOptionValue);
    }
}