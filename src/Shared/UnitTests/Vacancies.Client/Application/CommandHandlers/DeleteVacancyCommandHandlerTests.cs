using System.Threading;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers;

public class DeleteVacancyCommandHandlerTests
{
    private readonly Mock<ILogger<DeleteVacancyCommandHandler>> _mockLogger = new();
    private readonly Mock<IVacancyRepository> _mockVacancyRepository = new();
    private readonly Mock<IMessaging> _mockMessaging = new();
    private readonly Mock<ITimeProvider> _mockTimeProvider = new();

    [Theory]
    [InlineData(VacancyStatus.Live)]
    [InlineData(VacancyStatus.Approved)]
    [InlineData(VacancyStatus.Closed)]
    [InlineData(VacancyStatus.Submitted)]
    public async Task WhenVacancyIsNotInValidState_ShouldNotSetVacancyDeleted(VacancyStatus status)
    {
        var fixture = new Fixture();
        var vacancy = fixture.Build<Vacancy>().With(v => v.Status, status).Create();
        _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
        var sut = GetSut();
        await sut.Handle(fixture.Create<DeleteVacancyCommand>(), CancellationToken.None);
        _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
    }

    [Fact]
    public async Task WhenVacancyIsAlreadyDeleted_ShouldNotSetVacancyDeleted()
    {
        var fixture = new Fixture();
        var vacancy = fixture.Build<Vacancy>().With(v => v.IsDeleted, true).Create();
        _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync(vacancy);
        var sut = GetSut();
        await sut.Handle(fixture.Create<DeleteVacancyCommand>(), CancellationToken.None);
        _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
    }

    [Fact]
    public async Task WhenVacancyIsNotFound_ShouldNotSetVacancyDeleted()
    {
        var fixture = new Fixture();
        _mockVacancyRepository.Setup(r => r.GetVacancyAsync(It.IsAny<Guid>())).ReturnsAsync((Vacancy)null);
        var sut = GetSut();
        await sut.Handle(fixture.Create<DeleteVacancyCommand>(), CancellationToken.None);
        _mockVacancyRepository.Verify(m => m.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
    }

    private DeleteVacancyCommandHandler GetSut() =>
        new(
            _mockLogger.Object, _mockVacancyRepository.Object, _mockMessaging.Object, _mockTimeProvider.Object);
}