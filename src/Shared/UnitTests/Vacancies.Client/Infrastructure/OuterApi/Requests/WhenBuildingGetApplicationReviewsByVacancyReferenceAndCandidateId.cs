using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;
[TestFixture]
internal class WhenBuildingGetApplicationReviewsByVacancyReferenceAndCandidateId
{
    [Test, MoqAutoData]
    public void Then_Gets_Correct_Uri(long vacancyReference, Guid candidateId)
    {
        var actual = new GetApplicationReviewsByVacancyReferenceAndCandidateIdApiRequest(vacancyReference, candidateId);
        actual.GetUrl.Should().Be($"applicationReviews/{vacancyReference}/candidate/{candidateId}");
    }
}