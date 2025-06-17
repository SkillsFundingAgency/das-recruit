using System.Collections.Generic;
using AutoFixture;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using NUnit.Framework;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingPostVacancyReviewRequest
{
    [Test, RecursiveMoqAutoData]
    public void Then_The_Request_Is_Correctly_Built_And_Data_Populated(
        [Frozen]Mock<IEncodingService> encodingService)
    {
        encodingService.Setup(x => x.Decode(It.IsAny<string>(), It.IsAny<EncodingType>())).Returns(123456);
        var fixture = new Fixture();
        var vReview = fixture
            .Build<Recruit.Vacancies.Client.Domain.Entities.VacancyReview>()
            .With(c=>c.AutomatedQaOutcome, new RuleSetOutcome())
            .Create();
        
        var actual = new PostVacancyReviewRequest(vReview.Id, VacancyReviewDto.MapVacancyReviewDto(vReview, encodingService.Object));

        actual.PostUrl.Should().Be($"VacancyReviews/{vReview.Id}");
        ((VacancyReviewDto)actual.Data).Should().BeEquivalentTo(VacancyReviewDto.MapVacancyReviewDto(vReview, encodingService.Object));
    }
}