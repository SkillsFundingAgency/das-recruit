using System;
using Esfa.Recruit.Provider.Web.ViewModels.Reports;
using Esfa.Recruit.Provider.Web.ViewModels.Reports.ProviderApplicationsReport;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.ViewModels.Reports.ProviderApplicationsReport
{
    public class ProviderApplicationsReportCreateEditModelValidatorTests
    {
        [Fact]
        public void ShouldValidateDateRange()
        {
            var m = new ProviderApplicationsReportCreateEditModel {DateRange = null};

            var validator = GetValidator();

            var result = validator.Validate(m);

            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("You must select the time period for the report");
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData("32", "01", "2019")]
        [InlineData("30", "13", "2019")]
        [InlineData("30", "12", "201")]
        public void ShouldValidateFromDate(string day, string month, string year)
        {
            var m = new ProviderApplicationsReportCreateEditModel {
                DateRange = DateRangeType.Custom,
                FromDay = day,
                FromMonth = month,
                FromYear = year,
                ToDay = "1",
                ToMonth = "2",
                ToYear = "2020"
            };

            var validator = GetValidator();

            var result = validator.Validate(m);

            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("Date from format should be dd/mm/yyyy");
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData("32", "01", "2019")]
        [InlineData("30", "13", "2019")]
        [InlineData("30", "12", "201")]
        public void ShouldValidateToDate(string day, string month, string year)
        {
            var m = new ProviderApplicationsReportCreateEditModel {
                DateRange = DateRangeType.Custom,
                ToDay = day,
                ToMonth = month,
                ToYear = year,
                FromDay = "1",
                FromMonth = "2",
                FromYear = "2019"
            };

            var validator = GetValidator();

            var result = validator.Validate(m);

            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("Date to format should be dd/mm/yyyy");
        }

        [Fact]
        public void ShouldValidateToDateIsNotGreaterThanToday()
        {
            var m = new ProviderApplicationsReportCreateEditModel {
                DateRange = DateRangeType.Custom,
                FromDay = "1",
                FromMonth = "2",
                FromYear = "2019",
                ToDay = "06",
                ToMonth = "03",
                ToYear = "2019"
            };

            var validator = GetValidator();

            var result = validator.Validate(m);

            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("Date to cannot be in the future");
        }

        [Fact]
        public void ShouldValidateToDateIsGreaterThanFromDate()
        {
            var m = new ProviderApplicationsReportCreateEditModel {
                DateRange = DateRangeType.Custom,
                FromDay = "05",
                FromMonth = "03",
                FromYear = "2019",
                ToDay = "05",
                ToMonth = "03",
                ToYear = "2019"
            };

            var validator = GetValidator();

            var result = validator.Validate(m);

            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be("Date from must be earlier than Date to");
        }

        private ProviderApplicationsReportCreateEditModelValidator GetValidator()
        {
            var timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(t => t.NextDay).Returns(DateTime.Parse("2019-03-06"));
            var validator = new ProviderApplicationsReportCreateEditModelValidator(timeProvider.Object);
            return validator;
        }
    }
}
