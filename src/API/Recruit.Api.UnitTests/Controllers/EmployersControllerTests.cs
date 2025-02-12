using System.Collections.Generic;
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

public class EmployersControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly EmployersController _sut;
    private GetEmployerSummaryQuery _queryPassed = null;

    public EmployersControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMediator
            .Setup(x => x.Send(
                It.IsAny<GetEmployerSummaryQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEmployerSummaryResponse())
            .Callback<IRequest<GetEmployerSummaryResponse>, CancellationToken>((request, cancellationToken) =>
            {
                _queryPassed = (GetEmployerSummaryQuery)request;
            });

        _sut = new EmployersController(_mockMediator.Object);
    }

    [Xunit.Theory]
    [InlineData(" myjr4x")]
    [InlineData("MYJR4X")]
    [InlineData(" myjR4X ")]
    public async Task GetCall_EnsuresEmployerAccountIdPassedInQueryPassedToMediatorIsTrimmedAndUppercased(string input)
    {
        await _sut.Get(input);

        string.CompareOrdinal(_queryPassed.EmployerAccountId, "MYJR4X").Should().Be(0);
    }

    [Test, MoqAutoData]
    public async Task When_Getting_Successful_Applicants_Then_Query_Is_Created_And_Data_Returned(
        string employerAccountId,
        GetEmployerSuccessfulApplicantsQueryResponse response,
        List<SuccessfulApplicant> successfulApplicants,
        [Frozen] Mock<IMediator> mockMediator,
        [Greedy] EmployersController sut
    )
    {
        employerAccountId = employerAccountId.Trim().ToUpper();
        response.ResultCode = ResponseCode.Success;
        response.Data = successfulApplicants;

        mockMediator.Setup(x => x.Send(
                It.Is<GetEmployerSuccessfulApplicantsQuery>(q => q.EmployerAccountId == employerAccountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.GetSuccessfulApplicants(employerAccountId) as OkObjectResult;

        result.Should().NotBeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var actual = result.Value as List<SuccessfulApplicant>;
        actual.Should().BeEquivalentTo(successfulApplicants);
    }
}