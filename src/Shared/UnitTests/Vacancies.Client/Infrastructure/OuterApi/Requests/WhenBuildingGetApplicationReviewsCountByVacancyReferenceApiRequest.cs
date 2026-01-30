using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;
[TestFixture]
internal class WhenBuildingGetApplicationReviewsCountByVacancyReferenceApiRequest
{
    [Test, MoqAutoData]
    public void Then_The_Correct_Uri_Is_Returned(long vacancyReference)
    {
        string expectedUri = $"applicationReviews/vacancyReference/{vacancyReference}/count";
        var actual = new GetApplicationReviewsCountByVacancyReferenceApiRequest(vacancyReference);
        actual.GetUrl.Should().Be(expectedUri);
    }
}