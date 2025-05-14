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
            vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            
            Assert.ThrowsAsync<InvalidStateException>(() => orchestrator.GetEditVacancyDatesViewModelAsync(vacancyRouteModel,
                DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2)));
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Apprenticeship_Sets_ProgrammeName(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            List<IApprenticeshipProgramme> programmes,
            [Frozen] Mock<IRecruitVacancyClient> vacancyClient,
            [Frozen] Mock<IUtility> utility,
            EditVacancyDatesOrchestrator orchestrator)
        {
            vacancy.Status = VacancyStatus.Live;
            vacancy.IsDeleted = false;
            vacancy.ProgrammeId = programmes.FirstOrDefault().Id;
            vacancyClient.Setup(x => x.GetVacancyAsync(vacancyRouteModel.VacancyId.GetValueOrDefault()))
                .ReturnsAsync(vacancy);
            vacancyClient.Setup(x => x.GetActiveApprenticeshipProgrammesAsync()).ReturnsAsync(programmes);

            var actual = await orchestrator.GetEditVacancyDatesViewModelAsync(vacancyRouteModel,
                DateTime.UtcNow.AddMonths(1), DateTime.UtcNow.AddMonths(2));

            actual.Data.ProgrammeName.Should().Be(programmes.FirstOrDefault().Title);
            actual.Data.Title.Should().Be(vacancy.Title);
        }
    }
}