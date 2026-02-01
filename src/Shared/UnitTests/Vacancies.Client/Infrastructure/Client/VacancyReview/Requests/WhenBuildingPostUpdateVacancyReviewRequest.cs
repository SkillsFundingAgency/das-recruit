using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview.Requests;

public class WhenBuildingPostUpdateVacancyReviewRequest
{
    [Test, AutoData]
    public void Then_The_Request_Is_Correctly_Built_And_Data_Sent(
        [Frozen]Mock<IEncodingService> encodingService)
    {
        var fixture = new Fixture();
        var vReview = fixture
            .Build<Recruit.Vacancies.Client.Domain.Entities.VacancyReview>()
            .With(c=>c.AutomatedQaOutcome, new RuleSetOutcome())
            .Create();
        encodingService.Setup(x => x.Decode(vReview.VacancySnapshot.EmployerAccountId, EncodingType.AccountId)).Returns(123456);
        encodingService.Setup(x => x.Decode(vReview.VacancySnapshot.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(654321);
        
        var actual = new PostUpdateVacancyReviewRequest(VacancyReviewDto.MapVacancyReviewDto(vReview, encodingService.Object));

        actual.PostUrl.Should().Be($"VacancyReviews/{vReview.Id}/update");
        ((VacancyReviewDto)actual.Data).Should().BeEquivalentTo(new VacancyReviewDto
        {
            Id = vReview.Id,
            VacancyReference = vReview.VacancyReference,
            VacancyTitle = vReview.Title,
            CreatedDate = vReview.CreatedDate!.Value,
            SlaDeadLine = vReview.SlaDeadline!.Value,
            ReviewedDate = vReview.ReviewedDate!.Value,
            Status = vReview.Status.ToString(),
            SubmissionCount = (byte)vReview.SubmissionCount,
            ReviewedByUserEmail = vReview.ReviewedByUser?.Email,
            SubmittedByUserEmail = vReview.SubmittedByUser.Email,
            ClosedDate = vReview.ClosedDate,
            ManualOutcome = vReview.ManualOutcome?.ToString(),
            ManualQaComment = vReview.ManualQaComment,
            ManualQaFieldIndicators =vReview.ManualQaFieldIndicators.Where(c=>c.IsChangeRequested)
                .Select(c=>c.FieldIdentifier.ToString()).ToList(),
            AutomatedQaOutcome = vReview.AutomatedQaOutcome?.Decision.ToString(),
            AutomatedQaOutcomeIndicators = vReview.AutomatedQaOutcomeIndicators?.FirstOrDefault()?.IsReferred.ToString(),
            DismissedAutomatedQaOutcomeIndicators = vReview.DismissedAutomatedQaOutcomeIndicators,
            UpdatedFieldIdentifiers = vReview.UpdatedFieldIdentifiers,
            VacancySnapshot = JsonConvert.SerializeObject(vReview.VacancySnapshot),
            OwnerType = vReview.VacancySnapshot.OwnerType.ToString(),
            AccountId = 123456,
            Ukprn = vReview.VacancySnapshot.TrainingProvider.Ukprn!.Value,
            AccountLegalEntityId = 654321,
            EmployerName = vReview.VacancySnapshot.EmployerName,
            HashedAccountId = vReview.VacancySnapshot.EmployerAccountId,
            VacancyId = vReview.VacancySnapshot.Id,
            EmployerLocations = vReview.VacancySnapshot.EmployerLocations,
            EmployerLocationOption = vReview.VacancySnapshot.EmployerLocationOption
        });
    }
}