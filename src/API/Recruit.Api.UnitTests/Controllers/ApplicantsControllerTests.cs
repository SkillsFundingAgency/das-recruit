using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Recruit.Api.Controllers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;
using SFA.DAS.Testing.AutoFixture;
using Xunit;

namespace SFA.DAS.Recruit.Api.UnitTests.Controllers;

public class ApplicantsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly ApplicantsController _sut;
    private GetApplicantsQuery _queryPassed;

    public ApplicantsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMediator.Setup(x => x.Send(It.IsAny<GetApplicantsQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetApplicantsResponse())
            .Callback<IRequest<GetApplicantsResponse>, CancellationToken>((q, _) => _queryPassed = (GetApplicantsQuery)q);
        _sut = new ApplicantsController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetCall_EnsuresApplicantApplicationOutcomeFilterPassedToMediatorIsTrimmed()
    {
        var result = await _sut.Get(10000001, " successful ");
        _queryPassed.ApplicantApplicationOutcomeFilter.Contains(" ").Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task When_Getting_Application_Review_Then_Query_Is_Created_And_Data_Is_Returned(
        long vacancyReference,
        Guid candidateId,
        GetApplicationReviewResponse response,
        ApplicationReviewResponse applicationReview,
        [Frozen] Mock<IMediator> mockMediator,
        [Greedy] ApplicantsController sut
    )
    {
        response.ResultCode = ResponseCode.Success;
        response.Data = applicationReview;

        mockMediator.Setup(x => x.Send(
                It.Is<GetApplicationReviewQuery>(q => q.VacancyReference == vacancyReference && q.CandidateId == candidateId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.GetApplicant(vacancyReference, candidateId) as OkObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var actual = result.Value as ApplicationReviewResponse;
        actual.Should().BeEquivalentTo(applicationReview);
    }
}