using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest
{
	public class GetCloneableAuthorisedVacancyAsyncTests : CloneVacancyOrchestratorTestBase
    {
        [Fact]
        public async Task WhenRouteHasInvalidUkprn_ShouldThrowAuthorizationException()
        {
            var sut = GetSut(SourceVacancy);
            await Assert.ThrowsAsync<AuthorisationException>(()  => sut.GetCloneableAuthorisedVacancyAsync(new VacancyRouteModel{Ukprn=1234 }));
        }

        [Theory]
        [InlineData(VacancyStatus.Referred)]
        [InlineData(VacancyStatus.Draft)]         public async Task WhenVacancyInInvalidState_ShouldThrowInvalidStateException(VacancyStatus status)
        {
            var vacancy = SourceVacancy;
            vacancy.Status = status;
            var sut = GetSut(vacancy);
            await Assert.ThrowsAsync<InvalidStateException>(() => sut.GetCloneableAuthorisedVacancyAsync(VRM));
        }

        [Fact]
        public async Task WhenValidState_ShouldReturnVacancy()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = await sut.GetCloneableAuthorisedVacancyAsync(VRM);
            vacancy.Id.Should().Be(SourceVacancyId);
        }
    }
}