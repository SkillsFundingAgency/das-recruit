using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

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
            vacancyRepository.Setup(x => x.GetVacancyAsync(command.VacancyId)).ReturnsAsync(vacancy);
            outerApiGeocodeService.Setup(x => x.Geocode(It.IsAny<string>())).ReturnsAsync((Geocode)null);
            
            await handler.Handle(command, CancellationToken.None);
            
            vacancyRepository.Verify(x=>x.UpdateAsync(It.IsAny<Vacancy>()), Times.Never);
        }
    }
}