using System;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using FluentAssertions;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenConstructingGetVacancyMetricsRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Built(long vacancyReference, DateTime startDate, DateTime endDate)
    {
        var actual = new GetVacancyMetricsRequest(vacancyReference, startDate, endDate);

        actual.GetUrl.Should().Be($"metrics/vacancies/{vacancyReference}?startDate={startDate:yyyy-MM-ddTHH:mm:ss}&endDate={endDate:yyyy-MM-ddTHH:mm:ss}");
    }
}