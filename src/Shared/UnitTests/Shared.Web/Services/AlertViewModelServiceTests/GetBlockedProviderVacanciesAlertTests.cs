using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services.AlertViewModelServiceTests
{
    public class GetBlockedProviderVacanciesAlertTests : AlertViewModelServiceTestBase
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

            var vm = sut.GetBlockedProviderVacanciesAlert(vacancies, userLastDismissedDate);

            if (expectedNumberOfVacanciesReturned == null)
            {
                vm.Should().BeNull();
            }
            else
            {
                vm.ClosedVacancies.Count.Should().Be(expectedNumberOfVacanciesReturned);
            }
        }

        [Theory]
        [InlineData(null, "Provider A, Provider B and Provider C")]
        [InlineData("2019-07-15", "Provider B and Provider C")]
        [InlineData("2019-07-17", "Provider C")]
        public void ShouldReturnGroupedProviders(string userLastDismissedDateString, string expectedBlockedProviderNamesCaption)
        {
            var vacancies = GetVacancies();

            var userLastDismissedDate = GetUserDismissedDate(userLastDismissedDateString);

            var sut = new AlertViewModelService();

            var vm = sut.GetBlockedProviderVacanciesAlert(vacancies, userLastDismissedDate);

            vm.BlockedProviderNamesCaption.Should().Be(expectedBlockedProviderNamesCaption);
        }

        [Fact]
        public void ShouldReturnFormattedClosedVacancies()
        {
            var vacancies = GetVacancies();

            var sut = new AlertViewModelService();

            var vm = sut.GetBlockedProviderVacanciesAlert(vacancies, null);

            vm.ClosedVacancies.Count.Should().Be(5);
            vm.ClosedVacancies[0].Should().Be("A vacancy (VAC55555555)");
            vm.ClosedVacancies[1].Should().Be("B vacancy (VAC11111111)");
            vm.ClosedVacancies[2].Should().Be("C vacancy (VAC44444444)");
            vm.ClosedVacancies[3].Should().Be("D vacancy (VAC22222222)");
            vm.ClosedVacancies[4].Should().Be("E vacancy (VAC33333333)");
        }
        
        private IEnumerable<VacancySummary> GetVacancies()
        {
            return new List<VacancySummary>
            {
                CreateVacancy(DateTime.Parse("2019-07-14"), "B vacancy", 11111111, "Provider A"),
                CreateVacancy(DateTime.Parse("2019-07-15"), "D vacancy", 22222222, "Provider A"),
                CreateVacancy(DateTime.Parse("2019-07-16"), "E vacancy", 33333333, "Provider B"),
                CreateVacancy(DateTime.Parse("2019-07-17"), "C vacancy", 44444444, "Provider B"),
                CreateVacancy(DateTime.Parse("2019-07-18"), "A vacancy", 55555555, "Provider C")
            };
        }

        private VacancySummary CreateVacancy(DateTime closedDate, string title , long vacancyReference, string trainingProviderName)
        {
            return new VacancySummary
            {
                Status = VacancyStatus.Closed,
                ClosedDate = closedDate,
                ClosureReason = ClosureReason.BlockedByQa,
                Title = title,
                VacancyReference = vacancyReference,
                TrainingProviderName = trainingProviderName
            };
        }
    }
}
