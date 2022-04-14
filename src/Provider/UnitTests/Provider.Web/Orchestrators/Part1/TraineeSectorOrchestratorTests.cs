using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class TraineeOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task Then_Returns_Routes_In_The_View_Model(
            Vacancy vacancy,
            VacancyRouteModel vacancyRouteModel,
            List<IApprenticeshipRoute> routes,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            TraineeSectorOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.TraineeSector_Get))
                .ReturnsAsync(vacancy);
            recruitVacancyClient.Setup(x => x.GetApprenticeshipRoutes()).ReturnsAsync(routes);

            var actual = await orchestrator.GetTraineeViewModelAsync(vacancyRouteModel);

            actual.Routes.Should().BeEquivalentTo(routes.Select(c => new ApprenticeshipRouteViewModel
            {
                Id = c.Id,
                Name = c.Route
            }));
            actual.Title.Should().Be(vacancy.Title);
            actual.VacancyId.Should().Be(vacancy.Id);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }
    }
}