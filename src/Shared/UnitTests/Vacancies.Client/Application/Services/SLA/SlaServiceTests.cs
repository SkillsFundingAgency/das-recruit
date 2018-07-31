using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services.SLA;
using Esfa.Recruit.Vacancies.Client.Domain.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services.SLA
{
    public class SlaServiceTests
    {
        private const int SlaHours = 24;

        [Fact]
        public void GetSlaDeadlineAsync_ShouldHandleWorkingDays()
        {
            var bankholidaysServiceMock = new Mock<IBankHolidayService>();
            bankholidaysServiceMock.Setup(b => b.GetBankHolidaysAsync()).Returns(Task.FromResult(new List<DateTime>()));

            var sut = new SlaService(bankholidaysServiceMock.Object);

            var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-07-23 14:38")).Result;

            actual.Should().Be(DateTime.Parse("2018-07-24 14:38"));
        }

        [Fact]
        public void GetSlaDeadlineAsync_ShouldHandleWeekends()
        {
            var bankholidaysServiceMock = new Mock<IBankHolidayService>();
            bankholidaysServiceMock.Setup(b => b.GetBankHolidaysAsync()).Returns(Task.FromResult(new List<DateTime>()));

            var sut = new SlaService(bankholidaysServiceMock.Object);

            var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-07-27 14:38")).Result;

            actual.Should().Be(DateTime.Parse("2018-07-30 14:38"));
        }

        [Fact]
        public void GetSlaDeadlineAsync_ShouldHandleBankHolidays()
        {
            var bankholidaysServiceMock = new Mock<IBankHolidayService>();
            bankholidaysServiceMock.Setup(b => b.GetBankHolidaysAsync())
                .Returns(Task.FromResult(new List<DateTime> { DateTime.Parse("2018-08-27") }));

            var sut = new SlaService(bankholidaysServiceMock.Object);

            var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-08-24 14:38")).Result;

            actual.Should().Be(DateTime.Parse("2018-08-28 14:38"));
        }

        [Fact]
        public void GetSlaDeadlineAsync_ShouldHandleEaster()
        {
            var bankholidaysServiceMock = new Mock<IBankHolidayService>();
            bankholidaysServiceMock.Setup(b => b.GetBankHolidaysAsync())
                .Returns(Task.FromResult(new List<DateTime>
                {
                    DateTime.Parse("2019-04-19"),
                    DateTime.Parse("2019-04-22"),
                }));

            var sut = new SlaService(bankholidaysServiceMock.Object);

            var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2019-04-18 14:38")).Result;

            actual.Should().Be(DateTime.Parse("2019-04-23 14:38"));
        }

        [Fact]
        public void GetSlaDeadlineAsync_ShouldHandleSubmissionsOnNonWorkingDays()
        {
            var bankholidaysServiceMock = new Mock<IBankHolidayService>();
            bankholidaysServiceMock.Setup(b => b.GetBankHolidaysAsync()).Returns(Task.FromResult(new List<DateTime>()));

            var sut = new SlaService(bankholidaysServiceMock.Object);

            var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-07-21 14:38")).Result;

            actual.Should().Be(DateTime.Parse("2018-07-24 00:00"));
        }
    }
}
