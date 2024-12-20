using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest
{
	public class GetCloneableAuthorisedVacancyAsyncTests : CloneVacancyOrchestratorTestBase
    {
        [Test]
        public async Task WhenRouteHasInvalidEmployerAccountId_ShouldThrowAuthorizationException()
        {
            var sut = GetSut(SourceVacancy);

            var act = () => sut.GetCloneableAuthorisedVacancyAsync(new VacancyRouteModel { EmployerAccountId = "1234" });

            await act.Should().ThrowAsync<AuthorisationException>();
        }

        [TestCase(VacancyStatus.Referred)]
        [TestCase(VacancyStatus.Draft)]
        public async Task WhenVacancyInInvalidState_ShouldThrowInvalidStateException(VacancyStatus status)
        {
            var vacancy = SourceVacancy;
            vacancy.Status = status;
            var sut = GetSut(vacancy);
            
            var act = () => sut.GetCloneableAuthorisedVacancyAsync(VRM);
            
            await act.Should().ThrowAsync<InvalidStateException>();
        }

        [Test]
        public async Task WhenValidState_ShouldReturnVacancy()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = await sut.GetCloneableAuthorisedVacancyAsync(VRM);
            vacancy.Id.Should().Be(SourceVacancyId);
        }
    }
}