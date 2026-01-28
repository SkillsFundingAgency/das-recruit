using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;

[TestFixture]
internal class WhenBuildingGetVacancyAnalyticsByVacancyReferenceApiRequest
{
    [Test, MoqAutoData]
    public void GetVacancyAnalyticsByVacancyReferenceApiRequest_Should_Return_Correct_Url(long vacancyReference)
    {
        var request = new GetVacancyAnalyticsByVacancyReferenceApiRequest(vacancyReference);
        request.GetUrl.Should().Be($"vacancies/{vacancyReference}/analytics");
    }
}