using System.Linq;
using System.Threading;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.CommandHandlers
{
    public class GecodeVacancyCommandHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Gets_Geocode_Data_From_Postcode_When_Not_Anon(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocations = null;
            vacancy.EmployerLocation.Postcode = "TE1 5ET";
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode(vacancy.EmployerLocation.Postcode)).ReturnsAsync(apiResult);
            
            await handler.Handle(command, CancellationToken.None);
            
            vacancyRepository.Verify(x=>x.UpdateAsync(It.Is<Vacancy>(c=>
                c.EmployerLocation.Latitude.Equals(apiResult.Latitude)
                && c.EmployerLocation.Longitude.Equals(apiResult.Longitude)
                && c.GeoCodeMethod.Equals(apiResult.GeoCodeMethod)
            )), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Gets_Geocode_Data_From_Out_Code_When_Anon(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocations = null;
            vacancy.EmployerNameOption = EmployerNameOption.Anonymous;
            vacancy.EmployerLocation.Postcode = "TE1 5ET";
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode("TE1")).ReturnsAsync(apiResult);
            
            await handler.Handle(command, CancellationToken.None);
            
            vacancyRepository.Verify(x=>x.UpdateAsync(It.Is<Vacancy>(c=>
                c.EmployerLocation.Latitude.Equals(apiResult.Latitude)
                && c.EmployerLocation.Longitude.Equals(apiResult.Longitude)
                && c.GeoCodeMethod.Equals(apiResult.GeoCodeMethod)
                )), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_No_Location_Then_Geocode_Service_Not_Called(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocations = null;
            vacancy.EmployerLocation = null;
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            
            await handler.Handle(command, CancellationToken.None);
            
            vacancyRepository.Verify(x=>x.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
            outerApiGeocodeService.Verify(x=>x.Geocode(It.IsAny<string>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_If_No_Result_From_Service_Then_Vacancy_Not_Updated(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocations = null;
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode(It.IsAny<string>())).ReturnsAsync((Geocode)null);
            
            await handler.Handle(command, CancellationToken.None);
            
            vacancyRepository.Verify(x=>x.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Multiple_Locations_Are_Geocoded(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            vacancy.EmployerLocations.ForEach(location => outerApiGeocodeService.Setup(x => x.Geocode(location.Postcode)).ReturnsAsync(apiResult));
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            vacancy.EmployerLocations.Count.Should().BeGreaterThan(1);
            outerApiGeocodeService.Verify(x => x.Geocode(It.IsAny<string>()), Times.Exactly(vacancy.EmployerLocations.Count));
            vacancy.EmployerLocations.Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Anonymous_Vacancy_With_Multiple_Locations_Are_Geocoded(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.Anonymous;
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            vacancy.EmployerLocations.ForEach(location => outerApiGeocodeService.Setup(x => x.Geocode(location.PostcodeAsOutcode())).ReturnsAsync(apiResult));
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            vacancy.EmployerLocations.Count.Should().BeGreaterThan(1);
            outerApiGeocodeService.Verify(x => x.Geocode(It.IsAny<string>()), Times.Exactly(vacancy.EmployerLocations.Count));
            vacancy.EmployerLocations.Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Vacancy_With_Multiple_Similar_Locations_Are_Geocoded_In_A_Single_Call(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.EmployerLocations = [
                new Address { AddressLine1 = "address line 1-1", Postcode = "SW1A 2AA" },
                new Address { AddressLine1 = "address line 1-2", Postcode = "SW1A 2AA" },
            ];
            
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode("SW1A 2AA")).ReturnsAsync(apiResult);
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            outerApiGeocodeService.Verify(x => x.Geocode("SW1A 2AA"), Times.Once);
            vacancy.EmployerLocations.Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Anonymous_Vacancy_With_Multiple_Similar_Locations_Are_Geocoded_In_A_Single_Call(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.Anonymous;
            vacancy.EmployerLocations = [
                new Address { AddressLine1 = "address line 1-1", Postcode = "SW1A 2AA" },
                new Address { AddressLine1 = "address line 1-2", Postcode = "SW1A 2AA" },
            ];
            
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode("SW1A")).ReturnsAsync(apiResult);
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            outerApiGeocodeService.Verify(x => x.Geocode("SW1A"), Times.Once);
            vacancy.EmployerLocations.Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Locations_With_No_PostCode_Do_Not_Get_Geocoded(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.EmployerLocations = [
                new Address { AddressLine1 = "address line 1-1", Postcode = "SW1A 2AA" },
                new Address { AddressLine1 = "address line 1-2", Postcode = "SW2A 2AA" },
                new Address { AddressLine1 = "address line 1-3", Postcode = "" },
                new Address { AddressLine1 = "address line 1-4", Postcode = null },
            ];
            
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode("SW1A 2AA")).ReturnsAsync(apiResult);
            outerApiGeocodeService.Setup(x => x.Geocode("SW2A 2AA")).ReturnsAsync(apiResult);
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            outerApiGeocodeService.Verify(x => x.Geocode(It.IsAny<string>()), Times.Exactly(2));
            vacancy.EmployerLocations.Where(x => !string.IsNullOrWhiteSpace(x.Postcode)).Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            
            vacancy.EmployerLocations.Where(x => string.IsNullOrWhiteSpace(x.Postcode)).Should().AllSatisfy(x =>
            {
                x.Latitude.Should().BeNull();
                x.Longitude.Should().BeNull();
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Locations_With_No_Geocode_Result_Do_Not_Get_Geocoded(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.EmployerLocations = [
                new Address { AddressLine1 = "address line 1-1", Postcode = "SW1A 2AA" },
                new Address { AddressLine1 = "address line 1-2", Postcode = "SW2A 2AA" },
            ];
            
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode("SW1A 2AA")).ReturnsAsync(apiResult);
            outerApiGeocodeService.Setup(x => x.Geocode("SW2A 2AA")).ReturnsAsync((Geocode)null);
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            outerApiGeocodeService.Verify(x => x.Geocode(It.IsAny<string>()), Times.Exactly(2));
            vacancy.EmployerLocations.Where(x => x.Postcode == "SW1A 2AA").Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            
            vacancy.EmployerLocations.Where(x => x.Postcode == "SW2A 2AA").Should().AllSatisfy(x =>
            {
                x.Latitude.Should().BeNull();
                x.Longitude.Should().BeNull();
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Exceptions_When_Calling_Geocode_Do_Not_Fail_The_Process(
            GeocodeVacancyCommand command,
            Vacancy vacancy,
            Geocode apiResult,
            [Frozen] Mock<IVacancyRepository> vacancyRepository,
            [Frozen] Mock<IOuterApiGeocodeService> outerApiGeocodeService,
            GeocodeVacancyCommandHandler handler)
        {
            // arrange
            command.VacancyId = vacancy.Id;
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.EmployerLocations = [
                new Address { AddressLine1 = "address line 1-1", Postcode = "SW1A 2AA" },
                new Address { AddressLine1 = "address line 1-2", Postcode = "SW2A 2AA" },
            ];
            
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode("SW1A 2AA")).ReturnsAsync(apiResult);
            outerApiGeocodeService.Setup(x => x.Geocode("SW2A 2AA")).ThrowsAsync(new Exception());
            
            // act
            await handler.Handle(command, CancellationToken.None);
            
            // assert
            outerApiGeocodeService.Verify(x => x.Geocode(It.IsAny<string>()), Times.Exactly(2));
            vacancy.EmployerLocations.Where(x => x.Postcode == "SW1A 2AA").Should().AllSatisfy(x =>
            {
                x.Latitude.Should().Be(apiResult.Latitude);
                x.Longitude.Should().Be(apiResult.Longitude);
            });
            
            vacancy.EmployerLocations.Where(x => x.Postcode == "SW2A 2AA").Should().AllSatisfy(x =>
            {
                x.Latitude.Should().BeNull();
                x.Longitude.Should().BeNull();
            });
            vacancyRepository.Verify(x => x.UpdateAsync(vacancy), Times.Once);
        }
    }
}