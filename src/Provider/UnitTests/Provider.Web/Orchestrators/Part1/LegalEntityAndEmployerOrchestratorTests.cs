using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class LegalEntityAndEmployerOrchestratorTests
    {
        [Test, MoqAutoData]
        public void Then_If_There_Are_No_Permissions_Returned_Exception_Thrown(
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync((ProviderEditVacancyInfo)null);

            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                    "", 1));
        }


        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Permissions_Then_The_ViewModel_Is_Returned(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1);

            actual.Employers.Count().Should().Be(providerEditVacancyInfo.Employers.Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name }).Count());
            actual.VacancyId.Should().Be(vacancyRouteModel.VacancyId);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }

    }
}
