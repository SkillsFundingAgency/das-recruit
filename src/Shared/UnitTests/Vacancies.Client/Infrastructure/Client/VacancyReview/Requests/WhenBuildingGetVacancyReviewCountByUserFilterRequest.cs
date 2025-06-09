using System.Web;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetVacancyReviewCountByUserFilterRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Built_With_All_Parameters(string userId)
    {
        var actual = new GetVacancyReviewCountByUserFilterRequest(userId + "@%$£" + userId, true);
        
        actual.GetUrl.Should().Be($"users/{HttpUtility.UrlEncode(userId + "@%$£" + userId)}/VacancyReviews/count?approvedFirstTime=True");
    }
    
    [Test, AutoData]
    public void Then_The_Request_Is_Built_With_Optional_Parameters(string userId)
    {
        var actual = new GetVacancyReviewCountByUserFilterRequest(userId + "@%$£" + userId);
        
        actual.GetUrl.Should().Be($"users/{HttpUtility.UrlEncode(userId + "@%$£" + userId)}/VacancyReviews/count?approvedFirstTime=False");
    }
}