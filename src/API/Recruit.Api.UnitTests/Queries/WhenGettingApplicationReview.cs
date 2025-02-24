using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Recruit.Api.UnitTests.Queries;

public class GetApplicationReviewQueryHandlerTests
{
    [Test, MoqAutoData]
    public async Task Then_If_ApplicationReviewId_Is_Default_Then_Return_Invalid_Request(
        Mock<IApplicationReviewRepository> applicationReviewRepo,
        GetApplicationReviewQueryHandler sut
    )
    {
        var request = new GetApplicationReviewQuery(Guid.NewGuid(), Guid.Empty);

        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.InvalidRequest);

        result.ValidationErrors.First().Should().Be($"Invalid {nameof(request.ApplicationReviewId)}");

        applicationReviewRepo.Verify(x => x.GetAsync(It.IsAny<long>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_CandidateId_Is_Default_Then_Return_Invalid_Request(
        Mock<IApplicationReviewRepository> applicationReviewRepo,
        GetApplicationReviewQueryHandler sut
    )
    {
        var request = new GetApplicationReviewQuery( Guid.Empty, Guid.NewGuid());

        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.InvalidRequest);

        result.ValidationErrors.First().Should().Be($"Invalid {nameof(request.CandidateId)}");

        applicationReviewRepo.Verify(x => x.GetAsync(It.IsAny<long>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_ApplicationReview_Is_Null_Then_Return_Not_Found(
        GetApplicationReviewQuery request,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepo,
        GetApplicationReviewQueryHandler sut
    )
    {
        applicationReviewRepo.Setup(x => x.GetAsync(request.ApplicationReviewId)).ReturnsAsync(() => null);
            
        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.NotFound);

        result.ValidationErrors.Should().BeEmpty();

        applicationReviewRepo.Verify(x => x.GetAsync(request.ApplicationReviewId), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_When_ApplicationReview_Found_Returns_Success_And_Correctly_Mapped_Data(
        GetApplicationReviewQuery request,
        ApplicationReview applicationReview,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepo,
        GetApplicationReviewQueryHandler sut
    )
    {
        applicationReview.CandidateId = request.CandidateId;
        
        applicationReviewRepo.Setup(x => x.GetAsync(request.ApplicationReviewId)).ReturnsAsync(applicationReview);
            
        var result = await sut.Handle(request, CancellationToken.None);
        result.ResultCode.Should().Be(ResponseCode.Success);

        result.ValidationErrors.Should().BeEmpty();

        var actualResult = result.Data as ApplicationReviewResponse;
        actualResult.Should().NotBeNull();

        actualResult.ApplicationReviewId.Should().Be(applicationReview.Id);
        actualResult.ApplicationDate.Should().Be(applicationReview.Application.ApplicationDate);
        actualResult.VacancyReference.Should().Be(applicationReview.VacancyReference);
        actualResult.CandidateId.Should().Be(applicationReview.CandidateId);

        actualResult.BirthDate.Should().Be(applicationReview.Application.BirthDate);
        actualResult.FirstName.Should().Be(applicationReview.Application.FirstName);
        actualResult.LastName.Should().Be(applicationReview.Application.LastName);
        
        actualResult.Email.Should().Be(applicationReview.Application.Email);
        actualResult.Phone.Should().Be(applicationReview.Application.Phone);
        
        actualResult.AddressLine1.Should().Be(applicationReview.Application.AddressLine1);
        actualResult.AddressLine2.Should().Be(applicationReview.Application.AddressLine2);
        actualResult.AddressLine3.Should().Be(applicationReview.Application.AddressLine3);
        actualResult.AddressLine4.Should().Be(applicationReview.Application.AddressLine4);
        actualResult.Postcode.Should().Be(applicationReview.Application.Postcode);
        
        applicationReviewRepo.Verify(x => x.GetAsync(request.ApplicationReviewId), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_When_ApplicationReview_CandidateId_Does_Not_Match_Request_CandidateId_Then_Throws_Exception(
        GetApplicationReviewQuery request,
        ApplicationReview applicationReview,
        [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepo,
        GetApplicationReviewQueryHandler sut
    )
    {
        applicationReviewRepo.Setup(x => x.GetAsync(request.ApplicationReviewId)).ReturnsAsync(applicationReview);
            
        var action = () => sut.Handle(request, CancellationToken.None);
        await action.Should().ThrowAsync<Exception>();
    }
}