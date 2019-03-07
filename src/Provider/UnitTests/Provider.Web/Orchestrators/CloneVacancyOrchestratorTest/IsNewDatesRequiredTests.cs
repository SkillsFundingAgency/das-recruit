using System;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Provider.Web.Orchestrators.CloneVacancyOrchestratorTest
{
    public class IsNewDatesRequiredTests : CloneVacancyOrchestratorTestBase
    {
        [Fact]
        public void WhenStatusIsClosed_ThenReturnTrue()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = new Vacancy{Status = VacancyStatus.Closed};
            sut.IsNewDatesRequired(vacancy).Should().BeTrue();
        }

        [Fact]
        public void WhenStatusIsLiveAndDatesAreInFuture_ThenReturnFalse()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = new Vacancy{Status = VacancyStatus.Live, ClosingDate = DateTime.Now.AddDays(1)};
            sut.IsNewDatesRequired(vacancy).Should().BeFalse();
        }

        [Fact]
        public void WhenStatusIsLiveAndDatesAreInPast_ThenReturnTrue()
        {
            var sut = GetSut(SourceVacancy);
            var vacancy = new Vacancy{Status = VacancyStatus.Live, ClosingDate = DateTime.Now.AddDays(-1)};
            sut.IsNewDatesRequired(vacancy).Should().BeTrue();
        }

    }
}