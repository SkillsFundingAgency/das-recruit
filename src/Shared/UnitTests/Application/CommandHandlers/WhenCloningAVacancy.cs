using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.CommandHandlers;

public class WhenCloningAVacancy
{
    [Test, MoqAutoData]
    public async Task Then_Old_Style_Locations_Should_Be_Mapped_To_The_Multiple_Locations(
        CloneVacancyCommand command,
        Vacancy vacancy,
        [Frozen] Mock<IVacancyRepository> vacancyRepository,
        [Greedy] CloneVacancyCommandHandler sut)
    {
        // arrange
        vacancy.Status = VacancyStatus.Live;
        vacancyRepository
            .Setup(x => x.GetVacancyAsync(command.IdOfVacancyToClone))
            .ReturnsAsync(vacancy);

        Vacancy capturedVacancy = null;
        vacancyRepository
            .Setup(x => x.CreateAsync(It.IsAny<Vacancy>()))
            .Callback<Vacancy>(v => capturedVacancy = v);
        
        // act
        await sut.Handle(command, CancellationToken.None);

        // assert
        capturedVacancy.EmployerLocation.Should().BeNull();
        capturedVacancy.EmployerLocationOption.Should().Be(AvailableWhere.OneLocation);
        capturedVacancy.EmployerLocations.Should().HaveCount(1);
        capturedVacancy.EmployerLocations.Should().ContainEquivalentOf(vacancy.EmployerLocation);
    }

    [Test, MoqAutoData]
    public async Task Then_National_Vacancies_With_Missing_Location_Options_Are_Fixed(
        CloneVacancyCommand command,
        Vacancy vacancy,
        [Frozen] Mock<IVacancyRepository> vacancyRepository,
        [Greedy] CloneVacancyCommandHandler sut)
    {
        // arrange
        vacancy.Status = VacancyStatus.Live;
        vacancy.EmployerLocation = null;
        vacancy.EmployerLocationOption = null;
        vacancy.EmployerLocations = null;
        vacancyRepository
            .Setup(x => x.GetVacancyAsync(command.IdOfVacancyToClone))
            .ReturnsAsync(vacancy);

        Vacancy capturedVacancy = null;
        vacancyRepository
            .Setup(x => x.CreateAsync(It.IsAny<Vacancy>()))
            .Callback<Vacancy>(v => capturedVacancy = v);
        
        // act
        await sut.Handle(command, CancellationToken.None);

        // assert
        capturedVacancy.EmployerLocationOption.Should().Be(AvailableWhere.AcrossEngland);
    }
    
    [Test, MoqAutoData]
    public async Task Then_One_Location_Vacancies_With_Missing_Location_Options_Are_Fixed(
        CloneVacancyCommand command,
        Vacancy vacancy,
        [Frozen] Mock<IVacancyRepository> vacancyRepository,
        [Greedy] CloneVacancyCommandHandler sut)
    {
        // arrange
        vacancy.Status = VacancyStatus.Live;
        vacancy.EmployerLocation = null;
        vacancy.EmployerLocationOption = null;
        vacancy.EmployerLocationInformation = null;
        vacancy.EmployerLocations = [vacancy.EmployerLocations[0]];
        vacancyRepository
            .Setup(x => x.GetVacancyAsync(command.IdOfVacancyToClone))
            .ReturnsAsync(vacancy);

        Vacancy capturedVacancy = null;
        vacancyRepository
            .Setup(x => x.CreateAsync(It.IsAny<Vacancy>()))
            .Callback<Vacancy>(v => capturedVacancy = v);
        
        // act
        await sut.Handle(command, CancellationToken.None);

        // assert
        capturedVacancy.EmployerLocationOption.Should().Be(AvailableWhere.OneLocation);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Multiple_Location_Vacancies_With_Missing_Location_Options_Are_Fixed(
        CloneVacancyCommand command,
        Vacancy vacancy,
        [Frozen] Mock<IVacancyRepository> vacancyRepository,
        [Greedy] CloneVacancyCommandHandler sut)
    {
        // arrange
        vacancy.Status = VacancyStatus.Live;
        vacancy.EmployerLocation = null;
        vacancy.EmployerLocationOption = null;
        vacancy.EmployerLocationInformation = null;
        vacancyRepository
            .Setup(x => x.GetVacancyAsync(command.IdOfVacancyToClone))
            .ReturnsAsync(vacancy);

        Vacancy capturedVacancy = null;
        vacancyRepository
            .Setup(x => x.CreateAsync(It.IsAny<Vacancy>()))
            .Callback<Vacancy>(v => capturedVacancy = v);
        
        // act
        await sut.Handle(command, CancellationToken.None);

        // assert
        capturedVacancy.EmployerLocationOption.Should().Be(AvailableWhere.MultipleLocations);
    }
}