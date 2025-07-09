using System.Collections.Generic;
using AutoFixture;
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
                ), false
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
        vacancyDto.Status = ReviewStatus.PendingReview.ToString();
        UpdateToValidVacancyDto(vacancyDto, vacancy);
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference,"latest");
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
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, "latest");
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
    public async Task When_Calling_GetVacancyReviewsInProgressAsync_The_Request_Is_Made_And_List_Of_Vacancy_Reviews_Returned_By_ExpiredAssignationDateTime(
        DateTime expiredAssignationDateTime,
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
        var expectedRequest = new GetVacancyReviewByFilterRequest(expiredAssignationDateTime:expiredAssignationDateTime);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewByFilterRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse
            {
                VacancyReviews = [vacancyDto, vacancyDto2]
            });
        
        var actual = await service.GetVacancyReviewsInProgressAsync(expiredAssignationDateTime);
        
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
        vacancyDto.Status = ReviewStatus.PendingReview.ToString();
        UpdateToValidVacancyDto(vacancyDto, vacancy);
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference,"latestReferred");
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewResponse{ VacancyReview = vacancyDto});
        
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
        var expectedRequest = new GetVacancyReviewByVacancyReferenceAndReviewStatusRequest(vacancyReference, "latestReferred");
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewResponse>(It.Is<GetVacancyReviewByVacancyReferenceAndReviewStatusRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync((GetVacancyReviewResponse)null);
        
        var actual = await service.GetCurrentReferredVacancyReviewAsync(vacancyReference);
        
        actual.Should().BeNull();
    }

    [Test(Description = "Should throw as implemented by GetVacancyReviewSummary. To be removed when migration complete."), MoqAutoData]
    public void When_Calling_GetActiveAsync_Should_Throw_NotImplementedException(
        VacancyReviewService service)
    {
        Assert.ThrowsAsync<NotImplementedException>(service.GetActiveAsync);
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetVacancyReviewSummary_The_ApiClient_Is_Called_And_Response_Returned(
        GetVacancyReviewSummaryResponse apiResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        outerApiClient.Setup(x => x.Get<GetVacancyReviewSummaryResponse>(It.IsAny<GetVacancyReviewSummaryRequest>()))
            .ReturnsAsync(apiResponse);

        var actual = await service.GetVacancyReviewSummary();
        
        actual.Should().BeEquivalentTo(apiResponse);
    }
    
    [Test, MoqAutoData]
    public async Task When_Calling_GetApprovedCountAsync_The_ApiClient_Is_Called_And_Response_Returned(
        string userId,
        GetVacancyReviewCountResponse apiResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        var expectedRequest = new GetVacancyReviewCountByUserFilterRequest(userId);
        outerApiClient.Setup(x => x.Get<GetVacancyReviewCountResponse>(It.Is<GetVacancyReviewCountByUserFilterRequest>(c=>c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(apiResponse);

        var actual = await service.GetApprovedCountAsync(userId);
        
        actual.Should().Be(apiResponse.Count);
    }

    
    [Test, MoqAutoData]
    public async Task When_Calling_GetApprovedFirstTimeCountAsync_The_ApiClient_Is_Called_And_Response_Returned(
        string userId,
        GetVacancyReviewCountResponse apiResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        VacancyReviewService service)
    {
        var expectedRequest = new GetVacancyReviewCountByUserFilterRequest(userId, true);
        outerApiClient.Setup(x => x.Get<GetVacancyReviewCountResponse>(It.Is<GetVacancyReviewCountByUserFilterRequest>(c=>c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(apiResponse);

        var actual = await service.GetApprovedFirstTimeCountAsync(userId);
        
        actual.Should().Be(apiResponse.Count);
    }

    [Test, MoqAutoData]
    public async Task When_Calling_GetAssignedForUserAsync_The_ApiClient_Is_Called_And_Response_Returned(
        string userId,
        DateTime assignationExpiryDateTime,
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
        var expectedRequest = new GetVacancyReviewsAssignedToUserRequest(userId, assignationExpiryDateTime);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewListResponse>(It.Is<GetVacancyReviewsAssignedToUserRequest>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(new GetVacancyReviewListResponse
            {
                VacancyReviews = [vacancyDto, vacancyDto2]
            });
        
        var actual = await service.GetAssignedForUserAsync(userId, assignationExpiryDateTime);
        
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
    public async Task When_Calling_GetAnonymousApprovedCountAsync_The_ApiClient_Is_Called_And_Response_Returned(
        string accountLegalEntityPublicHashedId,
        long accountLegalEntityId,
        GetVacancyReviewCountResponse vacancyReviewCountResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        [Frozen] Mock<IEncodingService> encodingService,
        VacancyReviewService service)
    {
        encodingService.Setup(x => x.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
            .Returns(accountLegalEntityId);   
        var expectedRequest = new GetAnonymousApprovedCountByAccountLegalEntity(accountLegalEntityId);
        outerApiClient
            .Setup(
                x => x.Get<GetVacancyReviewCountResponse>(It.Is<GetAnonymousApprovedCountByAccountLegalEntity>(c => 
                    c.GetUrl == expectedRequest.GetUrl)))
            .ReturnsAsync(vacancyReviewCountResponse);
        
        var actual = await service.GetAnonymousApprovedCountAsync(accountLegalEntityPublicHashedId);

        actual.Should().Be(vacancyReviewCountResponse.Count);
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