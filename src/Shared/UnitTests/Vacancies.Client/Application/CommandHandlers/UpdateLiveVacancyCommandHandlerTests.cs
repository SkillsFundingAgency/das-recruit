using System.Threading;
using Esfa.Recruit.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Events;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers;

public class UpdateLiveVacancyCommandHandlerTests
{
    private Mock<ILogger<UpdateLiveVacancyCommandHandler>> _mockLogger;
    private Mock<IVacancyRepository> _mockRepository;
    private Mock<IMessaging> _mockMessaging;
    private Mock<ITimeProvider> _mockTimeProvider;
    private Mock<IOuterApiClient> _outerApiClient;
    private UpdateLiveVacancyCommandHandler _handler;
    private Guid _vacancyId = Guid.NewGuid();
        
    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UpdateLiveVacancyCommandHandler>>();
        _mockMessaging = new Mock<IMessaging>();
        _mockRepository = new Mock<IVacancyRepository>();
        _mockTimeProvider = new Mock<ITimeProvider>();
        _outerApiClient = new Mock<IOuterApiClient>();
        _mockTimeProvider
            .Setup(x => x.Now)
            .Returns(() => DateTime.UtcNow);

        _handler = new UpdateLiveVacancyCommandHandler(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockMessaging.Object,
            _mockTimeProvider.Object,
            _outerApiClient.Object);
    }

    [Test]
    [TestCase(LiveUpdateKind.ClosingDate)]
    [TestCase(LiveUpdateKind.ClosingDate | LiveUpdateKind.StartDate)]
    public async Task WhenLiveVacancyClosingDateHasChanged_ShouldPublishLiveVacancyClosingDateChangedEvent(LiveUpdateKind updateKind)
    {
        var user = new VacancyUser();
        var updatedVacancy = CreateVacancy();

        var message = new UpdateLiveVacancyCommand(updatedVacancy, user, updateKind);

        await _handler.Handle(message, CancellationToken.None);

        _mockMessaging
            .Verify(x => x.PublishEvent(
                It.Is<LiveVacancyClosingDateChangedEvent>(p =>
                    p.NewClosingDate == updatedVacancy.ClosingDate.Value
                    && p.VacancyId == _vacancyId
                    && p.VacancyReference == updatedVacancy.VacancyReference
                )));
        _outerApiClient.Verify(x => x.Post(It.IsAny<PostLiveVacancyUpdatedEventRequest>(), true), Times.Once);
    }

    [Test]
    [TestCase(LiveUpdateKind.StartDate)]
    [TestCase(LiveUpdateKind.None)]
    public async Task WhenClosingDateHasNotChanged_ShouldNotPublishLiveVacancyClosingDateChangedEvent(LiveUpdateKind updateKind)
    {
        var user = new VacancyUser();
        var updatedVacancy = CreateVacancy();

        var message = new UpdateLiveVacancyCommand(updatedVacancy, user, updateKind);

        await _handler.Handle(message, CancellationToken.None);

        _mockMessaging.Verify(x => x.PublishEvent(It.IsAny<LiveVacancyClosingDateChangedEvent>()), Times.Never);
        _outerApiClient.Verify(x => x.Post(It.IsAny<PostLiveVacancyUpdatedEventRequest>(), true), Times.Once);
    }

    private Vacancy CreateVacancy()
    {
        return new Vacancy
        {
            Id = _vacancyId,
            VacancyReference = 299792458,
            ClosingDate = DateTime.UtcNow
        };
    }
}