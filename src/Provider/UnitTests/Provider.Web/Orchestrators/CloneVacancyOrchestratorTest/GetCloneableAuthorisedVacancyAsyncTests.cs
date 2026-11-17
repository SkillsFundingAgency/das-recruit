using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest;

public class GetCloneableAuthorisedVacancyAsyncTests : CloneVacancyOrchestratorTestBase
{
    [Test]
    public Task WhenRouteHasInvalidUkprn_ShouldThrowAuthorizationException()
    {
        try
        {
            var sut = GetSut(SourceVacancy);
            Assert.ThrowsAsync<AuthorisationException>(() => sut.GetCloneableAuthorisedVacancyAsync(new VacancyRouteModel { Ukprn = 1234 }));
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    [TestCase(VacancyStatus.Referred)]
    [TestCase(VacancyStatus.Draft)]
    public Task WhenVacancyInInvalidState_ShouldThrowInvalidStateException(VacancyStatus status)
    {
        try
        {
            var vacancy = SourceVacancy;
            vacancy.Status = status;
            var sut = GetSut(vacancy);
            Assert.ThrowsAsync<InvalidStateException>(() => sut.GetCloneableAuthorisedVacancyAsync(VRM));
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    [Test]
    public async Task WhenValidState_ShouldReturnVacancy()
    {
        var sut = GetSut(SourceVacancy);
        var vacancy = await sut.GetCloneableAuthorisedVacancyAsync(VRM);
        vacancy.Id.Should().Be(SourceVacancyId);
    }
}