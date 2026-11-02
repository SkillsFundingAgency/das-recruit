using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Services;

public class SlaServiceTests
{
    [Fact]
    public void GetSlaDeadlineAsync_ShouldHandleWorkingDays()
    {
        var bankholidaysProviderMock = new Mock<IBankHolidayProvider>();
        bankholidaysProviderMock.Setup(b => b.GetBankHolidaysAsync()).Returns(Task.FromResult(new List<DateTime>()));

        var sut = new SlaService(bankholidaysProviderMock.Object);

        var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-07-23 14:38")).Result;

        actual.Should().Be(DateTime.Parse("2018-07-24 14:38"));
    }

    [Fact]
    public void GetSlaDeadlineAsync_ShouldHandleWeekends()
    {
        var bankholidaysProviderMock = new Mock<IBankHolidayProvider>();
        bankholidaysProviderMock.Setup(b => b.GetBankHolidaysAsync()).Returns(Task.FromResult(new List<DateTime>()));

        var sut = new SlaService(bankholidaysProviderMock.Object);

        var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-07-27 14:38")).Result;

        actual.Should().Be(DateTime.Parse("2018-07-30 14:38"));
    }

    [Fact]
    public void GetSlaDeadlineAsync_ShouldHandleBankHolidays()
    {
        var bankholidaysProviderMock = new Mock<IBankHolidayProvider>();
        bankholidaysProviderMock.Setup(b => b.GetBankHolidaysAsync())
            .Returns(Task.FromResult(new List<DateTime> { DateTime.Parse("2018-08-27") }));

        var sut = new SlaService(bankholidaysProviderMock.Object);

        var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-08-24 14:38")).Result;

        actual.Should().Be(DateTime.Parse("2018-08-28 14:38"));
    }

    [Fact]
    public void GetSlaDeadlineAsync_ShouldHandleEaster()
    {
        var bankholidaysProviderMock = new Mock<IBankHolidayProvider>();
        bankholidaysProviderMock.Setup(b => b.GetBankHolidaysAsync())
            .Returns(Task.FromResult(new List<DateTime>
            {
                DateTime.Parse("2019-04-19"),
                DateTime.Parse("2019-04-22"),
            }));

        var sut = new SlaService(bankholidaysProviderMock.Object);

        var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2019-04-18 14:38")).Result;

        actual.Should().Be(DateTime.Parse("2019-04-23 14:38"));
    }

    [Fact]
    public void GetSlaDeadlineAsync_ShouldHandleSubmissionsOnNonWorkingDays()
    {
        var bankholidaysProviderMock = new Mock<IBankHolidayProvider>();
        bankholidaysProviderMock.Setup(b => b.GetBankHolidaysAsync()).Returns(Task.FromResult(new List<DateTime>()));

        var sut = new SlaService(bankholidaysProviderMock.Object);

        var actual = sut.GetSlaDeadlineAsync(DateTime.Parse("2018-07-21 14:38")).Result;

        actual.Should().Be(DateTime.Parse("2018-07-24 00:00"));
    }
}