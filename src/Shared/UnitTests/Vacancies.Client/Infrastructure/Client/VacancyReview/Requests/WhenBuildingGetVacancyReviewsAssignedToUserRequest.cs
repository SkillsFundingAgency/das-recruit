using System.Web;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetVacancyReviewsAssignedToUserRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Built_Correctly(string userId, DateTime assignationExpiry)
    {
        var actual = new GetVacancyReviewsAssignedToUserRequest(userId + "@%$£" + userId, assignationExpiry);

        actual.GetUrl.Should().Be($"users/{HttpUtility.UrlEncode(userId + "@%$£" + userId)}/VacancyReviews?assignationExpiry={assignationExpiry}");
    }
}