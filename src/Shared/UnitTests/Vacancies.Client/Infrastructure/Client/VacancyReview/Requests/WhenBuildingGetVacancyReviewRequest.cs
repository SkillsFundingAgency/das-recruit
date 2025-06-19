using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetVacancyReviewRequest
{
    [Test, AutoData]

    public void Then_The_Request_Is_Built_Correctly(Guid vacancyReviewId)
    {
        var actual = new GetVacancyReviewRequest(vacancyReviewId);
        
        actual.GetUrl.Should().Be($"VacancyReviews/{vacancyReviewId}");
    }
}