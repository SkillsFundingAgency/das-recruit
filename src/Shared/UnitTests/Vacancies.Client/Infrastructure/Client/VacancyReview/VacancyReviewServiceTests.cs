using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.VacancyReview.Responses;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Encoding;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client.VacancyReview;

public class VacancyReviewServiceTests
{
    [Test, MoqAutoData]
    public async Task When_Calling_CreateAsync_The_Data_Is_Mapped_And_Request_Made(
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IEncodingService> encodingService,
        VacancyReviewService service)
    {
        encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId)).Returns(1001);
        encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId)).Returns(1002);
        var model = BuildVacancyReviewEntity();
        var expectedRequest = new PostVacancyReviewRequest(model.Id,VacancyReviewDto.MapVacancyReviewDto(model, encodingService.Object));
        
        await service.CreateAsync(model);
        
        outerApiClient.Verify(x=>x.Post(
            It.Is<PostVacancyReviewRequest>(
                c=>c.PostUrl == expectedRequest.PostUrl
                && ((VacancyReviewDto)c.Data).VacancyTitle == ((VacancyReviewDto)expectedRequest.Data).VacancyTitle
                ), true
            ), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task When_Calling_UpdateAsync_The_Data_Is_Mapped_And_Request_Made(
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IEncodingService> encodingService,
        VacancyReviewService service)
    {
        var model = BuildVacancyReviewEntity();
        encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId)).Returns(1001);
        encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.PublicAccountLegalEntityId)).Returns(1002);
        var expectedRequest = new PostVacancyReviewRequest(model.Id, VacancyReviewDto.MapVacancyReviewDto(model, encodingService.Object));
        
        await service.UpdateAsync(model);
        
        outerApiClient.Verify(x=>x.Post(
            It.Is<PostVacancyReviewRequest>(
                c=>c.PostUrl == expectedRequest.PostUrl
                   && ((VacancyReviewDto)c.Data).VacancyTitle == ((VacancyReviewDto)expectedRequest.Data).VacancyTitle
            ), false
        ), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task When_Calling_GetAsync_The_Request_Is_Made_And_VacancyReview_Mapped_To_Entity(
        Guid reviewId,
        VacancyReviewDto vacancyDto,
        Vacancy vacancy,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        vacancyDto.Status = ReviewStatus.PendingReview.ToString();
        UpdateToValidVacancyDto(vacancyDto, vacancy);
        var expectedRequest = new GetVacancyReviewRequest(reviewId);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewResponse>(It.Is<GetVacancyReviewRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewResponse{ VacancyReview = vacancyDto});
        
        var actual = await service.GetAsync(reviewId);
        
        actual.Should()
            .BeEquivalentTo((Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto, options => 
                options.Excluding(c=>c.AutomatedQaOutcomeIndicators)
            );
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetAsync_The_Request_Is_Made_And_Null_Returned_If_Not_Found(
        Guid reviewId,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        var expectedRequest = new GetVacancyReviewRequest(reviewId);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewResponse>(It.Is<GetVacancyReviewRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync((GetVacancyReviewResponse)null);
        
        var actual = await service.GetAsync(reviewId);
        
        actual.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetLatestReviewByReferenceAsync_The_Request_Is_Made_And_VacancyReview_Mapped_To_Entity(
        long vacancyReference,
        VacancyReviewDto vacancyDto,
        Vacancy vacancy,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        vacancyDto.ManualOutcome = null;
        vacancyDto.Status = ReviewStatus.PendingReview.ToString();
        vacancyDto.VacancyReference = vacancyReference;

        UpdateToValidVacancyDto(vacancyDto, vacancy);

        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse{ VacancyReviews = [vacancyDto]});
        
        var actual = await service.GetLatestReviewByReferenceAsync(vacancyReference);
        
        actual.Should()
            .BeEquivalentTo((Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto, options => 
                options.Excluding(c=>c.AutomatedQaOutcomeIndicators)
            );
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetLatestReviewByReferenceAsync_The_Request_Is_Made_And_Null_Returned_If_Not_Found(
        long vacancyReference,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse{ VacancyReviews = []});
        
        var actual = await service.GetLatestReviewByReferenceAsync(vacancyReference);
        
        actual.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetForVacancyAsync_The_Request_Is_Made_And_List_Of_Vacancy_Reviews_Returned(
        long vacancyReference,
        VacancyReviewDto vacancyDto,
        VacancyReviewDto vacancyDto2,
        Vacancy vacancy,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        vacancyDto.Status = ReviewStatus.PendingReview.ToString();
        vacancyDto2.Status = ReviewStatus.PendingReview.ToString();
        UpdateToValidVacancyDto(vacancyDto, vacancy);
        UpdateToValidVacancyDto(vacancyDto2, vacancy);
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse
            {
                VacancyReviews = [vacancyDto, vacancyDto2]
            });
        
        var actual = await service.GetForVacancyAsync(vacancyReference);
        
        actual.Should()
            .BeEquivalentTo(new List<Recruit.Vacancies.Client.Domain.Entities.VacancyReview>
                {
                    (Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto,
                    (Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto2
                }, options => 
                options.Excluding(c=>c.AutomatedQaOutcomeIndicators)
            );
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetByStatusAsync_The_Request_Is_Made_And_List_Of_Vacancy_Reviews_Returned_By_Status(
        ReviewStatus reviewStatus,
        VacancyReviewDto vacancyDto,
        VacancyReviewDto vacancyDto2,
        Vacancy vacancy,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        vacancyDto.Status = ReviewStatus.PendingReview.ToString();
        vacancyDto2.Status = ReviewStatus.PendingReview.ToString();
        UpdateToValidVacancyDto(vacancyDto, vacancy);
        UpdateToValidVacancyDto(vacancyDto2, vacancy);
        var expectedRequest = new GetVacancyReviewByFilterRequest([reviewStatus]);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByFilterRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse
            {
                VacancyReviews = [vacancyDto, vacancyDto2]
            });
        
        var actual = await service.GetByStatusAsync(reviewStatus);
        
        actual.Should()
            .BeEquivalentTo(new List<Recruit.Vacancies.Client.Domain.Entities.VacancyReview>
                {
                    (Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto,
                    (Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto2
                }, options => 
                    options.Excluding(c=>c.AutomatedQaOutcomeIndicators)
            );
    }
    
    [Test, MoqAutoData]
    public async Task When_Calling_GetCurrentReferredVacancyReviewAsync_The_Request_Is_Made_And_VacancyReview_Mapped_To_Entity(
        long vacancyReference,
        VacancyReviewDto vacancyDto,
        Vacancy vacancy,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        vacancyDto.ManualOutcome = null;
        vacancyDto.Status = ReviewStatus.Closed.ToString();
        UpdateToValidVacancyDto(vacancyDto, vacancy);
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, ReviewStatus.Closed);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse { VacancyReviews = [vacancyDto]});
        
        var actual = await service.GetCurrentReferredVacancyReviewAsync(vacancyReference);
        
        actual.Should()
            .BeEquivalentTo((Recruit.Vacancies.Client.Domain.Entities.VacancyReview)vacancyDto, options => 
                options.Excluding(c=>c.AutomatedQaOutcomeIndicators)
            );
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetCurrentReferredVacancyReviewAsync_The_Request_Is_Made_And_Null_Returned_If_Not_Found(
        long vacancyReference,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, ReviewStatus.Closed);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync((GetVacancyReviewListResponse)null);
        
        var actual = await service.GetCurrentReferredVacancyReviewAsync(vacancyReference);
        
        actual.Should().BeNull();
    }

    private static void UpdateToValidVacancyDto(VacancyReviewDto vacancyDto, Vacancy vacancy)
    {
        vacancyDto.VacancySnapshot = JsonConvert.SerializeObject(vacancy);
        vacancyDto.ManualOutcome = "Referred";
        vacancyDto.AutomatedQaOutcome = "Approve";
    }

    private Recruit.Vacancies.Client.Domain.Entities.VacancyReview BuildVacancyReviewEntity()
    {
        var fixture = new Fixture();
        return fixture
            .Build<Recruit.Vacancies.Client.Domain.Entities.VacancyReview>()
            .With(c=>c.AutomatedQaOutcome, new RuleSetOutcome())
            .Create();
    }
}