using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingGetLatestVacancyReviewByVacancyReferenceRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Correctly_Built(long vacancyReference, ReviewStatus reviewStatus)
    {
        var actual = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, reviewStatus);
        
        actual.GetUrl.Should().Be($"vacancies/{vacancyReference}/vacancyReviews?status={reviewStatus}");
    }
}