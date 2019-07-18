using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Shared.Web.Services.AlertViewModelServiceTests
{
    public class GetTransferredVacanciesAlertTests : AlertViewModelServiceTestBase
    {
        [Theory]
        [InlineData(null, 5)]
        [InlineData("2019-07-16", 2)]
        [InlineData("2019-07-18", null)]
        public void ShouldReturnNonDismissedTransferredVacanciesByReason(string userLastDismissedDateString, int? expectedNumberOfVacanciesReturned)
        {
            var vacancies = GetVacancies();

            var userLastDismissedDate = GetUserDismissedDate(userLastDismissedDateString);

            var sut = new AlertViewModelService();

            var vm = sut.GetTransferredVacanciesAlert(vacancies, TransferReason.BlockedByQa, userLastDismissedDate);

            if (expectedNumberOfVacanciesReturned == null)
            {
                vm.Should().BeNull();
            }
            else
            {
                vm.TransferredVacanciesCount.Should().Be(expectedNumberOfVacanciesReturned);
            }
        }

        [Theory]
        [InlineData(null, "Provider A, Provider B and Provider C")]
        [InlineData("2019-07-15", "Provider B and Provider C")]
        [InlineData("2019-07-17", "Provider C")]
        public void ShouldReturnGroupedProviders(string userLastDismissedDateString, string expectedProviderNamesCaption)
        {
            var vacancies = GetVacancies();

            var userLastDismissedDate = GetUserDismissedDate(userLastDismissedDateString);

            var sut = new AlertViewModelService();

            var vm = sut.GetTransferredVacanciesAlert(vacancies, TransferReason.BlockedByQa, userLastDismissedDate);

            vm.ProviderNamesCaption.Should().Be(expectedProviderNamesCaption);
        }

        private IEnumerable<VacancySummary> GetVacancies()
        {
            return new List<VacancySummary>
            {
                CreateVacancy(TransferReason.BlockedByQa, DateTime.Parse("2019-07-14"), "Provider A"),
                CreateVacancy(TransferReason.BlockedByQa, DateTime.Parse("2019-07-15"), "Provider A"),
                CreateVacancy(TransferReason.BlockedByQa, DateTime.Parse("2019-07-16"), "Provider B"),
                CreateVacancy(TransferReason.BlockedByQa, DateTime.Parse("2019-07-17"), "Provider B"),
                CreateVacancy(TransferReason.BlockedByQa, DateTime.Parse("2019-07-18"), "Provider C"),
                CreateVacancy(TransferReason.EmployerRevokedPermission, DateTime.Parse("2019-07-18"), "Should not be included"),
            };
        }

        private VacancySummary CreateVacancy(TransferReason reason, DateTime transferredDate, string providerName)
        {
            return new VacancySummary
            {
                TransferInfoReason = reason,
                TransferInfoTransferredDate = transferredDate,
                TransferInfoProviderName = providerName
            };
        }
    }
}
