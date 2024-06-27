using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenConstructingGetListOfVacanciesWithMetricsRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Built(long vacancyReference, DateTime startDate, DateTime endDate)
    {
        var actual = new GetListOfVacanciesWithMetricsRequest(startDate, endDate);

        actual.GetUrl.Should().Be($"metrics/vacancy?startDate={startDate}&endDate={endDate}");
    }
}