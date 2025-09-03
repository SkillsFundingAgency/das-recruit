using System.Threading;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.ViewModels.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Commands.ManageNotifications;
using Esfa.Recruit.Vacancies.Client.Application.Queries.ManageNotifications.GetEmployerNotificationPreferences;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators.ManageNotificationsOrchestratorTests;

public class WhenUpdatingNewNotificationPreferences
{
    [Test, MoqAutoData]
    public async Task It_Fails_When_User_Not_Found(
        string employerAccountId,
        VacancyUser user,
        ManageNotificationsEditModelEx editModel,
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
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeFalse();
        capturedQuery.Should().NotBeNull();
        capturedQuery.IdamsId.Should().Be(user.UserId);
    }
    
    [Test]
    [MoqInlineAutoData(nameof(NotificationFrequencyEx.Never), NotificationScopeEx.NotSet, NotificationFrequencyEx.Never)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.UserSubmittedVacancies), NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.NotSet)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.OrganisationVacancies), NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.NotSet)]
    public async Task For_VacancyApprovedOrUpdatedByDfE_Event_It_Succeeds_When_User_Found(
        string optionValue,
        NotificationScopeEx expectedScope,
        NotificationFrequencyEx expectedFrequency,
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
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never }
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        UpdateUserNotificationPreferencesCommand? capturedCommand = null;
        mediator
            .Setup(x => x.Send(It.IsAny<UpdateUserNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest x, CancellationToken _) => capturedCommand = x as UpdateUserNotificationPreferencesCommand);

        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedValue = "Never",
            ApplicationSubmittedFrequencyValue = "Weekly",
            VacancyApprovedOrRejectedValue = optionValue,
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeTrue();
        capturedCommand.Should().NotBeNull();
        capturedCommand.Id.Should().Be(response.Id);
        
        var vac = capturedCommand.NotificationPreferences.GetForEvent(NotificationTypesEx.VacancyApprovedOrRejected);
        vac.Scope.Should().Be(expectedScope);
        vac.Frequency.Should().Be(expectedFrequency);
    }
    
    [Test]
    [MoqInlineAutoData(nameof(NotificationFrequencyEx.Never), nameof(NotificationFrequencyEx.Never), NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Never)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Never), NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Never)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Daily), NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Daily)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Weekly), NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Weekly)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.UserSubmittedVacancies), nameof(NotificationFrequencyEx.Immediately), NotificationScopeEx.UserSubmittedVacancies, NotificationFrequencyEx.Immediately)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Never), NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Never)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Daily), NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Daily)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Weekly), NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Weekly)]
    [MoqInlineAutoData(nameof(NotificationScopeEx.OrganisationVacancies), nameof(NotificationFrequencyEx.Immediately), NotificationScopeEx.OrganisationVacancies, NotificationFrequencyEx.Immediately)]
    public async Task For_ApplicationSubmitted_Event_It_Succeeds_When_User_Found(
        string optionValue,
        string frequencyOptionValue,
        NotificationScopeEx expectedScope,
        NotificationFrequencyEx expectedFrequency,
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
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never }
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        UpdateUserNotificationPreferencesCommand? capturedCommand = null;
        mediator
            .Setup(x => x.Send(It.IsAny<UpdateUserNotificationPreferencesCommand>(), It.IsAny<CancellationToken>()))
            .Callback((IRequest x, CancellationToken _) => capturedCommand = x as UpdateUserNotificationPreferencesCommand);

        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedValue = optionValue,
            ApplicationSubmittedFrequencyValue = frequencyOptionValue,
            VacancyApprovedOrRejectedValue = "Never",
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeTrue();
        capturedCommand.Should().NotBeNull();
        capturedCommand.Id.Should().Be(response.Id);
        var app = capturedCommand.NotificationPreferences.GetForEvent(NotificationTypesEx.ApplicationSubmitted);
        app.Scope.Should().Be(expectedScope);
        app.Frequency.Should().Be(expectedFrequency);
    }

    [Test]
    [MoqInlineAutoData(nameof(NotificationScopeEx.UserSubmittedVacancies))]
    [MoqInlineAutoData(nameof(NotificationScopeEx.OrganisationVacancies))]
    public async Task It_Fails_When_User_Found_But_Frequency_Not_Selected(
        string optionValue,
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
                    new NotificationPreference { Event = NotificationTypesEx.ApplicationSubmitted, Scope = NotificationScopeEx.OrganisationVacancies, Frequency = NotificationFrequencyEx.Daily },
                    new NotificationPreference { Event = NotificationTypesEx.VacancyApprovedOrRejected, Scope = NotificationScopeEx.NotSet, Frequency = NotificationFrequencyEx.Never }
                ]
            }
        };
        
        mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerNotificationPreferencesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        var editModel = new ManageNotificationsEditModelEx()
        {
            ApplicationSubmittedValue = optionValue,
            ApplicationSubmittedFrequencyValue = null,
            VacancyApprovedOrRejectedValue = "Never",
        };
        
        // act
        var result = await sut.NewUpdateUserNotificationPreferencesAsync(editModel, user);

        // assert
        result.Success.Should().BeFalse();
        result.Errors.Errors.Should().HaveCount(1);
    }
}