using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using FluentAssertions;
using Xunit;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using System.Threading.Tasks;

namespace Esfa.Recruit.UnitTests.Employer.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public class GetCloneVacancyDatesQuestionViewModelAsyncTests : CloneVacancyOrchestratorTestBase
    {
        [Fact]
        public async Task WhenClosingDateIsInPast_ThenThrowInvalidStateException()
        {
            var vacancy = SourceVacancy;
            vacancy.Status = VacancyStatus.Live;
            vacancy.ClosingDate = DateTime.UtcNow.AddDays(-1);

            var sut = GetSut(vacancy);

            await Assert.ThrowsAsync<InvalidStateException>(() => sut.GetCloneVacancyDatesQuestionViewModelAsync(VRM));
        }

        [Fact]
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
}