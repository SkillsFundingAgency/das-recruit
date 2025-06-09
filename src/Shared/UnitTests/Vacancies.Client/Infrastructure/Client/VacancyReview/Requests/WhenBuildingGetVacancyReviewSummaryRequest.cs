using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetVacancyReviewSummaryRequest
{
    [Test]
    public void Then_The_Url_Is_Correct()
    {
        var actual = new GetVacancyReviewSummaryRequest();
        
        actual.GetUrl.Should().Be("VacancyReviews/summary");
    }
}