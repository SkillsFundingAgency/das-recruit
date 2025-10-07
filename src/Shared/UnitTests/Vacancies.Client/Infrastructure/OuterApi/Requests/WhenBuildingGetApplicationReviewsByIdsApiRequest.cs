using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using NUnit.Framework;
using static Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests.GetApplicationReviewsByIdsApiRequest;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi.Requests;
[TestFixture]
internal class WhenBuildingGetApplicationReviewsByIdsApiRequest
{
    [Test, MoqAutoData]
    public void Then_Gets_Correct_Url(GetApplicationReviewsByIdsApiRequestData payload)
    {
        var request = new GetApplicationReviewsByIdsApiRequest(payload);
        request.PostUrl.Should().Be("applicationReviews/manyByApplicationIds");
        request.Data.Should().BeEquivalentTo(payload);
    }
}
