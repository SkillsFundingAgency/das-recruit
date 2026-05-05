using System.Threading;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.EventHandlers;

[TestFixture]
internal class ArchiveVacancyCommandHandlerTests
{
    private Mock<ILogger<ArchiveVacancyCommandHandler>> _loggerMock;
    private Mock<IVacancyRepository> _repositoryMock;
    private Mock<ITimeProvider> _timeProviderMock;

    private ArchiveVacancyCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ArchiveVacancyCommandHandler>>();
        _repositoryMock = new Mock<IVacancyRepository>();
        _timeProviderMock = new Mock<ITimeProvider>();

        _handler = new ArchiveVacancyCommandHandler(
            _loggerMock.Object,
            _repositoryMock.Object,
            _timeProviderMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Unit_When_Vacancy_Not_Found()
    {
        // Arrange
        var command = new ArchiveVacancyCommand { VacancyId = Guid.NewGuid() };

        _repositoryMock
            .Setup(r => r.GetVacancyAsync(command.VacancyId))
            .ReturnsAsync((Vacancy)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
    }

    [Test]
    public async Task Handle_Should_Not_Archive_When_CanArchive_Is_False()
    {
        // Arrange
        var command = new ArchiveVacancyCommand { VacancyId = Guid.NewGuid() };

        var vacancy = new Vacancy
        {
            Id = command.VacancyId,
            Status = VacancyStatus.Live
        };

        _repositoryMock
            .Setup(r => r.GetVacancyAsync(command.VacancyId))
            .ReturnsAsync(vacancy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        vacancy.Status.Should().Be(VacancyStatus.Live);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
    }

    [Test]
    public async Task Handle_Should_Archive_Vacancy_When_Valid()
    {
        // Arrange
        var command = new ArchiveVacancyCommand { VacancyId = Guid.NewGuid() };

        var now = DateTime.UtcNow;

        var vacancy = new Vacancy
        {
            Id = command.VacancyId,
            Status = VacancyStatus.Closed
        };

        _repositoryMock
            .Setup(r => r.GetVacancyAsync(command.VacancyId))
            .ReturnsAsync(vacancy);

        _timeProviderMock
            .Setup(t => t.Now)
            .Returns(now);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        vacancy.Status.Should().Be(VacancyStatus.Archived);
        vacancy.LastUpdatedDate.Should().Be(now);

        _repositoryMock.Verify(r => r.UpdateAsync(vacancy), Times.Once);
    }
}