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
}