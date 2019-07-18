using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services.AlertViewModelServiceTests
{
    public class GetWithdrawnVacanciesAlertTests : AlertViewModelServiceTestBase
    {
        [Theory]
        [InlineData(null, 5)]
        [InlineData("2019-07-16", 2)]
        [InlineData("2019-07-18", null)]
        public void ShouldReturnNonDismissedVacancies(string userLastDismissedDateString, int? expectedNumberOfVacanciesReturned)
        {
            var vacancies = GetVacancies();

            var userLastDismissedDate = GetUserDismissedDate(userLastDismissedDateString);

            var sut = new AlertViewModelService();

            var vm = sut.GetWithdrawnByQaVacanciesAlert(vacancies, userLastDismissedDate);

            if (expectedNumberOfVacanciesReturned == null)
            {
                vm.Should().BeNull();
            }
            else
            {
                vm.ClosedVacancies.Count.Should().Be(expectedNumberOfVacanciesReturned);
            }
        }

        [Fact]
        public void ShouldReturnFormattedClosedVacancies()
        {
            var vacancies = GetVacancies();

            var sut = new AlertViewModelService();

            var vm = sut.GetWithdrawnByQaVacanciesAlert(vacancies, null);

            vm.ClosedVacancies.Count.Should().Be(5);
            vm.ClosedVacancies[0].Should().Be("first vacancy (VAC11111111)");
            vm.ClosedVacancies[1].Should().Be("second vacancy (VAC22222222)");
            vm.ClosedVacancies[2].Should().Be("third vacancy (VAC33333333)");
            vm.ClosedVacancies[3].Should().Be("fourth vacancy (VAC44444444)");
            vm.ClosedVacancies[4].Should().Be("fifth vacancy (VAC55555555)");
        }

        private IEnumerable<VacancySummary> GetVacancies()
        {
            return new List<VacancySummary>
            {
                CreateVacancy(DateTime.Parse("2019-07-14"), "first vacancy", 11111111),
                CreateVacancy(DateTime.Parse("2019-07-15"), "second vacancy", 22222222),
                CreateVacancy(DateTime.Parse("2019-07-16"), "third vacancy", 33333333),
                CreateVacancy(DateTime.Parse("2019-07-17"), "fourth vacancy", 44444444),
                CreateVacancy(DateTime.Parse("2019-07-18"), "fifth vacancy", 55555555)
            };
        }

        private VacancySummary CreateVacancy(DateTime closedDate, string title, long vacancyReference)
        {
            return new VacancySummary
            {
                Status = VacancyStatus.Closed,
                ClosedDate = closedDate,
                ClosureReason = ClosureReason.WithdrawnByQa,
                Title = title,
                VacancyReference = vacancyReference
            };
        }
    }
}
