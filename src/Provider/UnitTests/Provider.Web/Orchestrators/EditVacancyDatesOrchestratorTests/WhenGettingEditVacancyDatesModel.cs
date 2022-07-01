using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.EditVacancyDatesOrchestratorTests
{
    public class WhenGettingEditVacancyDatesModel
    {
        [Test, MoqAutoData]
        public void Then_Cannot_Be_Extended_If_Status_Is_Not_Live(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            List<IApprenticeshipProgramme> programmes,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            [Frozen] Mock<IUtility> utility,
            EditVacancyDatesOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Submitted;
            vacancy.IsDeleted = false;
            vacancy.VacancyType = VacancyType.Apprenticeship;
            vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            
            Assert.ThrowsAsync<InvalidStateException>(() => orchestrator.GetEditVacancyDatesViewModelAsync(vacancyRouteModel,
                DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2)));
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Apprenticeship_Sets_ProgrammeName_And_Not_Route(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            List<IApprenticeshipProgramme> programmes,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            [Frozen] Mock<IUtility> utility,
            EditVacancyDatesOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Live;
            vacancy.IsDeleted = false;
            vacancy.VacancyType = VacancyType.Apprenticeship;
            vacancy.ProgrammeId = programmes.FirstOrDefault().Id;
            vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            vacancyClient.Setup(x => x.GetActiveApprenticeshipProgrammesAsync()).ReturnsAsync(programmes);

            var actual = await orchestrator.GetEditVacancyDatesViewModelAsync(vacancyRouteModel,
                DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2));

            actual.Data.VacancyType.Should().Be(vacancy.VacancyType);
            actual.Data.ProgrammeName.Should().Be(programmes.FirstOrDefault().Title);
            actual.Data.RouteName.Should().BeNullOrEmpty();
            actual.Data.Title.Should().Be(vacancy.Title);

        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Traineeship_Sets_Route_And_Not_ProgrammeName(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            List<IApprenticeshipRoute> routes,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            [Frozen] Mock<IUtility> utility,
            EditVacancyDatesOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Live;
            vacancy.IsDeleted = false;
            vacancy.VacancyType = VacancyType.Traineeship;
            vacancy.RouteId = routes.FirstOrDefault().Id;
            vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            vacancyClient.Setup(x => x.GetApprenticeshipRoutes()).ReturnsAsync(routes);

            var actual = await orchestrator.GetEditVacancyDatesViewModelAsync(vacancyRouteModel,
                DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2));

            actual.Data.VacancyType.Should().Be(vacancy.VacancyType);
            actual.Data.ProgrammeName.Should().BeNullOrEmpty();
            actual.Data.RouteName.Should().Be(routes.FirstOrDefault().Route);
            actual.Data.Title.Should().Be(vacancy.Title);
        }
    }
}