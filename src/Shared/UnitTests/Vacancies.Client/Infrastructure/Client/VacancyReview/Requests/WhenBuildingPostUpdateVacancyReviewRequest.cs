using AutoFixture;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingPostUpdateVacancyReviewRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Correctly_Built_And_Data_Sent()
    {
        var fixture = new Fixture();
        var vReview = fixture
            .Build<Recruit.Vacancies.Client.Domain.Entities.VacancyReview>()
            .With(c=>c.AutomatedQaOutcome, new RuleSetOutcome())
            .Create();
        
        var actual = new PostUpdateVacancyReviewRequest((VacancyReviewDto)vReview);

        actual.PostUrl.Should().Be($"VacancyReviews/{vReview.Id}/update");
        ((VacancyReviewDto)actual.Data).Should().BeEquivalentTo((VacancyReviewDto)vReview);
    }
}