using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ArchiveVacancy;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Vacancies;

[TestFixture]
internal class WhenArchivingVacancy
{
    private Mock<IProviderVacancyClient> _providerClientMock;
    private Mock<IRecruitVacancyClient> _recruitClientMock;
    private Mock<IUtility> _utilityMock;

    private ArchiveVacancyOrchestrator _sut;

    [SetUp]
    public void Setup()
    {
        _providerClientMock = new Mock<IProviderVacancyClient>();
        _recruitClientMock = new Mock<IRecruitVacancyClient>();
        _utilityMock = new Mock<IUtility>();

        _sut = new ArchiveVacancyOrchestrator(
            _providerClientMock.Object,
            _recruitClientMock.Object,
            _utilityMock.Object);
    }

    [Test, MoqAutoData]
    public async Task GetArchiveViewModelAsync_Should_Return_ViewModel_When_Valid(
        Guid vacancyId,
        long ukprn,
        VacancyRouteModel vrm,
        Vacancy vacancy)
    {
        // Arrange
        vrm.VacancyId = vacancyId;
        vrm.Ukprn = ukprn;

        vacancy.Id = vacancyId;
        vacancy.Status = VacancyStatus.Closed;
        vacancy.IsDeleted = false;

        _recruitClientMock
            .Setup(x => x.GetVacancyAsync(vacancyId))
            .ReturnsAsync(vacancy);
        _utilityMock
            .Setup(x => x.CheckAuthorisedAccess(vacancy, ukprn)).Verifiable();
        // Act
        var result = await _sut.GetArchiveViewModelAsync(vrm);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            vacancy.Title,
            vacancy.Status,
            vacancy.VacancyReference,
            vacancy.EmployerName,
            vrm.Ukprn,
            vrm.VacancyId
        });

        _utilityMock.Verify(x => x.CheckAuthorisedAccess(vacancy, vrm.Ukprn), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetArchiveViewModelAsync_Should_Throw_When_CannotArchive(Guid vacancyId,
        long ukprn,
        VacancyRouteModel vrm,
        Vacancy vacancy)
    {
        // Arrange
        vrm.VacancyId = vacancyId;
        vrm.Ukprn = ukprn;

        vacancy.Id = vacancyId;
        vacancy.Status = VacancyStatus.Live;
        vacancy.IsDeleted = false;

        _recruitClientMock
            .Setup(x => x.GetVacancyAsync(vacancyId))
            .ReturnsAsync(vacancy);
        _utilityMock
            .Setup(x => x.CheckAuthorisedAccess(vacancy, ukprn)).Verifiable();

        // Act
        Func<Task> act = async () => await _sut.GetArchiveViewModelAsync(vrm);

        // Assert
        await act.Should().ThrowAsync<InvalidStateException>();

        _providerClientMock.Verify(x => x.ArchiveVacancyAsync(It.IsAny<Guid>(), It.IsAny<VacancyUser>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task GetArchiveViewModelAsync_Should_Throw_When_NotAuthorised(Guid vacancyId,
        long ukprn,
        VacancyRouteModel vrm,
        Vacancy vacancy)
    {
        // Arrange
        vrm.VacancyId = vacancyId;
        vrm.Ukprn = ukprn;

        vacancy.Id = vacancyId;
        vacancy.Status = VacancyStatus.Live;
        vacancy.IsDeleted = false;

        _recruitClientMock
            .Setup(x => x.GetVacancyAsync(vacancyId))
            .ReturnsAsync(vacancy);

        _utilityMock
            .Setup(x => x.CheckAuthorisedAccess(vacancy, vrm.Ukprn))
            .Throws(new UnauthorizedAccessException());

        // Act
        Func<Task> act = async () => await _sut.GetArchiveViewModelAsync(vrm);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test, MoqAutoData]
    public async Task ArchiveVacancyAsync_Should_Call_Client_And_Return_ViewModel(Guid vacancyId,
        long ukprn,
        VacancyUser user,
        ArchiveViewModel model,
        VacancyRouteModel vrm,
        Vacancy vacancy)
    {
        // Arrange
        vrm.VacancyId = vacancyId;
        vrm.Ukprn = ukprn;

        model.VacancyId = vacancyId;
        model.Ukprn = ukprn;

        vacancy.Id = vacancyId;
        vacancy.Status = VacancyStatus.Closed;
        vacancy.IsDeleted = false;

        _recruitClientMock
            .Setup(x => x.GetVacancyAsync(vacancyId))
            .ReturnsAsync(vacancy);

        // Act
        var result = await _sut.ArchiveVacancyAsync(model, user);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            vacancy.Title,
            vacancy.Status,
            vacancy.EmployerName,
            vacancy.VacancyReference,
            model.Ukprn,
            model.VacancyId
        });

        _providerClientMock.Verify(
            x => x.ArchiveVacancyAsync(vacancyId, user),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task ArchiveVacancyAsync_Should_Throw_When_CannotArchive(Guid vacancyId,
        long ukprn,
        VacancyUser user,
        ArchiveViewModel model,
        VacancyRouteModel vrm,
        Vacancy vacancy)
    {
        // Arrange
        vrm.VacancyId = vacancyId;
        vrm.Ukprn = ukprn;

        model.VacancyId = vacancyId;
        model.Ukprn = ukprn;

        vacancy.Id = vacancyId;
        vacancy.Status = VacancyStatus.Live;
        vacancy.IsDeleted = false;

        _recruitClientMock
            .Setup(x => x.GetVacancyAsync(vacancyId))
            .ReturnsAsync(vacancy);

        // Act
        Func<Task> act = async () => await _sut.ArchiveVacancyAsync(model, new VacancyUser());

        // Assert
        await act.Should().ThrowAsync<InvalidStateException>();

        _providerClientMock.Verify(
            x => x.ArchiveVacancyAsync(It.IsAny<Guid>(), It.IsAny<VacancyUser>()),
            Times.Never);
    }
}
