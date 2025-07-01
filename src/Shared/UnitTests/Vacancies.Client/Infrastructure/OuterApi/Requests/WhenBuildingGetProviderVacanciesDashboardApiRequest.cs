using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenBuildingGetProviderVacanciesDashboardApiRequest
{
    [Test, AutoData]
    public void Then_It_Is_Correctly_Constructed(
        int ukprn,
        List<ApplicationReviewStatus> status,
        int pageNumber)
    {
        var actual = new GetProviderDashboardVacanciesApiRequest(ukprn, pageNumber, status);

        actual.GetUrl.Should().Be($"providers/dashboard/{ukprn}/vacancies?pageNumber={pageNumber}&status={string.Join("&status=", status)}");
    }
}