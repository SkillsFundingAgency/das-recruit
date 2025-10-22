using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;
[TestFixture]
internal class WhenBuildingGetApplicationReviewsByVacancyReferenceAndTempStatusApiRequest
{
    [Test, MoqAutoData]
    public void Then_The_Correct_Uri_Is_Created(long vacancyReference, ApplicationReviewStatus status)
    {
        var actual = new GetApplicationReviewsByVacancyReferenceAndTempStatusApiRequest(vacancyReference, status);
        actual.GetUrl.Should().Be($"applicationReviews/vacancyReference/{vacancyReference}/temp-status/{status}");
    }
}
