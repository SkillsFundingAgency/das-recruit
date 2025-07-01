using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class WhenBuildingGetEmployerVacanciesDashboardApiRequest
{
    [Test, AutoData]
    public void Then_It_Is_Correctly_Constructed(
        int accountId,
        List<ApplicationReviewStatus> status,
        int pageNumber)
    {
        var actual = new GetEmployerDashboardVacanciesApiRequest(accountId, pageNumber, status);

        actual.GetUrl.Should().Be($"employerAccounts/{accountId}/dashboard/vacancies?pageNumber={pageNumber}&status={string.Join("&status=", status)}");
    }
}