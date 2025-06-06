using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest;

public class GetCloneVacancyDatesQuestionViewModelAsyncTests : CloneVacancyOrchestratorTestBase
{
    [Test]
    public async Task WhenClosingDateIsInPast_ThenThrowInvalidStateException()
    {
        var vacancy = SourceVacancy;
        vacancy.Status = VacancyStatus.Live;
        vacancy.ClosingDate = DateTime.UtcNow.AddDays(-1);

        var sut = GetSut(vacancy);
        
        // act/assert
        var func = async () => await sut.GetCloneVacancyDatesQuestionViewModelAsync(VRM);
        await func.Should().ThrowAsync<InvalidStateException>();
    }

    [Test]
    public async Task ThenReturnsViewModelWithDates()
    {
        var vacancy = SourceVacancy;
        vacancy.Status = VacancyStatus.Live;
        var expectedStartDate = SourceStartDate.AsGdsDate();
        var expectedClosingDate = SourceClosingDate.AsGdsDate();

        var sut = GetSut(vacancy);

        var vm = await sut.GetCloneVacancyDatesQuestionViewModelAsync(VRM);

        vm.StartDate.Should().Be(expectedStartDate);
        vm.ClosingDate.Should().Be(expectedClosingDate);
    }
}